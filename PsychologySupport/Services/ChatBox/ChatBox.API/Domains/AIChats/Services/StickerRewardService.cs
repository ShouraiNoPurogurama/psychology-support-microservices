using System.Text.Json;
using BuildingBlocks.Exceptions;
using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Models;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Domains.AIChats.Services;

/// <summary>
/// Service để xử lý logic "Claim" (nhận) sticker thưởng.
/// </summary>
public class StickerRewardService(
    ChatBoxDbContext dbContext,
    IAIProvider aiProvider,
    ISessionConcurrencyManager concurrencyManager, 
    ILogger<StickerRewardService> logger) : IStickerRewardService
{
    public async Task<AIMessageResponseDto> ClaimStickerAsync(Guid userId)
    {
        // 1. Tìm reward đang chờ (đọc nhanh, không tracking)
        // Chúng ta cần RewardId và SessionId để acquire lock
        var pendingRewardInfo = await dbContext.PendingStickerRewards
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.IsClaimed == false)
            .Select(r => new { r.RewardId, r.SessionId })
            .FirstOrDefaultAsync();

        if (pendingRewardInfo == null)
        {
            throw new NotFoundException("Không tìm thấy phần thưởng sticker nào đang chờ.", "REWARD_NOT_FOUND");
        }

        // 2. Acquire lock trên Session
        // Ngăn chặn user vừa gửi tin nhắn vừa claim reward cùng lúc
        var lockAcquired = await concurrencyManager.TryAcquireSessionLockAsync(
            pendingRewardInfo.SessionId, TimeSpan.FromSeconds(15));

        if (!lockAcquired)
        {
            throw new TimeoutException("Hệ thống đang bận, không thể nhận thưởng lúc này. Vui lòng thử lại sau.");
        }

        // Bắt đầu transaction
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // 3. Lấy reward (lần 2, bên trong lock và transaction)
            var rewardToClaim = await dbContext.PendingStickerRewards
                .FirstOrDefaultAsync(r => r.RewardId == pendingRewardInfo.RewardId && r.IsClaimed == false);

            // Kiểm tra lại, phòng trường hợp race condition
            if (rewardToClaim == null)
            {
                await transaction.RollbackAsync(); // Không cần commit
                throw new NotFoundException("Sticker đã được nhận trong khi bạn chờ.", "REWARD_ALREADY_CLAIMED");
            }
            
            // 4. Kiểm tra session có tồn tại và thuộc về user
            var targetSession = await dbContext.AIChatSessions
                .AsNoTracking()
                .AnyAsync(s => s.Id == rewardToClaim.SessionId && s.UserId == userId && s.IsActive == true);
            
            if (!targetSession)
            {
                throw new ForbiddenException("Không thể nhận thưởng cho session không tồn tại hoặc không thuộc về bạn.");
            }

            // 5. Gọi LLM (Emo)
            // Chuẩn bị prompt đặc biệt để Emo giải thích về sticker
            // Dựa trên yêu cầu của bạn: "mô tả lại ý nghĩa dựa trên filter_prompt"
            string llmContext =
                $"[Bối cảnh hệ thống]: Emo, hãy tạo một tin nhắn để tặng người dùng sticker. Sticker này được tạo từ bối cảnh: '{rewardToClaim.PromptFilter}'. " +
                "Hãy giải thích ngắn gọn (1-2 câu) ý nghĩa của sticker này và gửi tặng họ. " +
                "Hãy tỏ ra vui vẻ và thấu hiểu. KHÔNG dùng markdown.";

            // Chúng ta dùng lại IAIProvider để nó tự động "enrich context"
            // với lịch sử chat gần đây, y như bạn gợi ý.
            var aiPayload = new AIRequestPayload(
                Context: llmContext,
                Summarization: null, // IAIProvider (GeminiProvider) sẽ tự điền
                HistoryMessages: []  // IAIProvider (GeminiProvider) sẽ tự điền
            );

            var emoFollowUpText = await aiProvider.GenerateResponseAsync(aiPayload, rewardToClaim.SessionId);

            if (string.IsNullOrWhiteSpace(emoFollowUpText))
            {
                throw new Exception("Emo không thể tạo tin nhắn giới thiệu sticker lúc này.");
            }
            
            // 6. Tạo nội dung tin nhắn JSON
            var messageContentPayload = new
            {
                type = "reward_sticker",
                text = emoFollowUpText.Trim(),
                sticker_url = rewardToClaim.StickerUrl
            };
            string jsonContent = JsonSerializer.Serialize(messageContentPayload);

            // 7. Lấy BlockNumber tiếp theo (re-implement logic từ MessageProcessor)
            int lastBlockNumber = await dbContext.AIChatMessages
                .Where(m => m.SessionId == rewardToClaim.SessionId)
                .MaxAsync(m => (int?)m.BlockNumber) ?? 0;
            
            // 8. Tạo Entity tin nhắn mới
            var newMessage = new AIMessage
            {
                Id = Guid.NewGuid(),
                SenderIsEmo = true,
                SenderUserId = null,
                SessionId = rewardToClaim.SessionId,
                Content = jsonContent,
                CreatedDate = DateTimeOffset.UtcNow,
                IsRead = false, // Tin nhắn mới
                BlockNumber = lastBlockNumber + 1
            };

            // 9. Cập nhật và lưu (trong transaction)
            rewardToClaim.Claim(); // Đánh dấu đã nhận
            dbContext.PendingStickerRewards.Update(rewardToClaim);
            await dbContext.AIChatMessages.AddAsync(newMessage);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation("User {UserId} claimed Reward {RewardId}. New message {MessageId} created in Session {SessionId}.",
                userId, rewardToClaim.RewardId, newMessage.Id, rewardToClaim.SessionId);

            // 10. Trả về DTO
            // Map từ AIMessage (Entity) sang AIMessageResponseDto
            return new AIMessageResponseDto(
                newMessage.SessionId,
                newMessage.SenderIsEmo,
                newMessage.Content,
                newMessage.CreatedDate
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Failed to claim sticker for user {UserId}", userId);
            throw; // Re-throw để global exception handler bắt
        }
        finally
        {
            // Quan trọng: Luôn giải phóng lock
            concurrencyManager.ReleaseSessionLock(pendingRewardInfo.SessionId);
        }
    }
}