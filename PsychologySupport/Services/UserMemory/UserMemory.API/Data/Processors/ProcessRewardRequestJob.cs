using System.Text.Json;
using BuildingBlocks.Messaging.Events.IntegrationEvents.UserMemory;
using Microsoft.EntityFrameworkCore;
using Quartz;
using UserMemory.API.Models;
using UserMemory.API.Shared.Dtos;
using UserMemory.API.Shared.Enums;
using UserMemory.API.Shared.Outbox;
using UserMemory.API.Shared.Services.Contracts;

namespace UserMemory.API.Data.Processors
{
    /// <summary>
    /// Job chạy nền (Quartz.NET) để xử lý các tin nhắn outbox 'RewardRequested'.
    /// Job này là "Người Điều Phối": Lấy request, gọi LLM, gọi Media Service, và cập nhật kết quả.
    /// </summary>
    [DisallowConcurrentExecution] // Đảm bảo không có 2 job chạy song song xử lý cùng 1 message
    public sealed class ProcessRewardRequestJob(
        UserMemoryDbContext dbContext,
        IRewardPromptGenerator promptGenerator,
        IMediaServiceClient mediaServiceClient,
        IOutboxWriter outboxWriter,
        ILogger<ProcessRewardRequestJob> logger)
        : IJob
    {
        private const int BatchSize = 10; // Xử lý 10 message mỗi lần chạy

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("Starting ProcessRewardRequestJob at {Time}", DateTimeOffset.UtcNow);

            // 1. Poll (quét) DB
            var messagesToProcess = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOn == null &&
                            m.Type == typeof(RewardRequestedIntegrationEvent).FullName) // Dùng tên type an toàn
                .OrderBy(m => m.OccurredOn) // Ưu tiên xử lý cái cũ nhất
                .Take(BatchSize)
                .ToListAsync(context.CancellationToken);

            if (!messagesToProcess.Any())
            {
                logger.LogInformation("No new reward requests to process.");
                return;
            }

            logger.LogInformation("Found {Count} reward requests to process.", messagesToProcess.Count);

            // 2. Xử lý từng message
            foreach (var message in messagesToProcess)
            {
                // Xử lý mỗi message trong 1 transaction RENG BIỆT.
                // Nếu 1 message lỗi, nó không ảnh hưởng đến các message khác trong batch.
                await using var transaction = await dbContext.Database.BeginTransactionAsync(context.CancellationToken);
            
                Reward? reward = null; // Biến để lưu reward entity

                try
                {
                    // 3. Lấy dữ liệu (Deserialize)
                    var evt = JsonSerializer.Deserialize<RewardRequestedIntegrationEvent>(message.Content);
                    if (evt == null)
                    {
                        throw new InvalidOperationException($"Failed to deserialize message content for ID: {message.Id}");
                    }

                    // Lấy Reward entity từ DB
                    reward = await dbContext.Rewards.FindAsync(new object[] { evt.RewardId }, context.CancellationToken);
                    if (reward == null)
                    {
                        throw new InvalidOperationException($"Reward not found: {evt.RewardId}");
                    }

                    // Kiểm tra trạng thái: Nếu job đã chạy rồi nhưng bị crash, không chạy lại
                    if (reward.Status != RewardStatus.Pending)
                    {
                        logger.LogWarning("Reward {RewardId} is not in Pending state (Actual: {Status}). Skipping.", reward.Id, reward.Status);
                    }
                    else
                    {
                        // Đánh dấu đang xử lý
                        reward.Status = RewardStatus.Processing;
                        await dbContext.SaveChangesAsync(context.CancellationToken); // Lưu trạng thái "Processing"
                
                        // 4. Gọi LLM#3 (Prompt Gen)
                        RewardGenerationDataDto generationData = await promptGenerator.PrepareGenerationDataAsync(evt.AliasId, evt.RewardId, evt.ChatSessionId, context.CancellationToken);
                    
                        // Lưu lại prompt đã dùng
                        reward.PromptBase = generationData.PromptBase;
                        reward.PromptFiller = generationData.PromptFiller;

                        // 5. Gọi Media Service
                        MediaGenerationResultDto mediaResult = await mediaServiceClient.GenerateStickerAsync(generationData.FinalPrompt, reward.Id, context.CancellationToken);
                    
                        // 6. (Chờ...) Nhận { cdn_url } - Đã xong ở bước 5

                        // 7. Cập nhật kết quả
                        reward.Status = RewardStatus.Granted;
                        reward.StickerUrl = mediaResult.CdnUrl;
                    
                        logger.LogInformation("Successfully processed reward {RewardId}.", reward.Id);

                        // 8. Tạo event mới (RewardGranted)
                        var grantedEvent = new RewardGrantedIntegrationEvent(reward.Id, reward.AliasId, generationData.UserId, evt.ChatSessionId ,reward.StickerUrl, reward.PromptFiller);
                        await outboxWriter.WriteAsync(grantedEvent, context.CancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process reward request for message {MessageId}.", message.Id);
                
                    // 10. Xử lý Lỗi
                    if (reward != null)
                    {
                        reward.Status = RewardStatus.Failed;
                        // TODO: Có thể lưu chi tiết lỗi vào reward.Meta
                    
                        // Tạo event RewardFailed
                        var failedEvent = new RewardFailedEvent(reward.Id, reward.AliasId, ex.Message);
                        await outboxWriter.WriteAsync(failedEvent, context.CancellationToken);
                    }
                }
                finally
                {
                    // 9. Đánh dấu hoàn tất (LUÔN LUÔN chạy)
                    // Dù thành công hay thất bại, message này cũng đã được "xử lý".
                    // Chúng ta không muốn retry một message bị lỗi vĩnh viễn.
                    message.ProcessedOn = DateTimeOffset.UtcNow;

                    // Lưu tất cả thay đổi: 
                    // (UPDATE rewards SET status=...) + 
                    // (INSERT INTO outbox (RewardGranted/RewardFailed)) +
                    // (UPDATE outbox_messages SET processed_on=...)
                    await dbContext.SaveChangesAsync(context.CancellationToken);
                
                    // Commit transaction cho message này
                    await transaction.CommitAsync(context.CancellationToken);
                }
            }
        }
    }
}