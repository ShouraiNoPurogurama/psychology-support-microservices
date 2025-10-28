using System.Text.Json;
using BuildingBlocks.Exceptions;
using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Domains.AIChats.Services;

/// <summary>
/// Service để xử lý logic "Claim" (nhận) sticker thưởng.
/// - Không gọi Rollback thủ công ở các nhánh; chỉ Commit khi thành công.
/// - Nếu có exception, dispose transaction sẽ tự rollback.
/// - Khóa/tra cứu theo sessionId mà client truyền vào để nhất quán.
/// </summary>
public class StickerRewardService(
    ChatBoxDbContext dbContext,
    IAIProvider aiProvider,
    ISessionConcurrencyManager concurrencyManager,
    ILogger<StickerRewardService> logger) : IStickerRewardService
{
    public async Task<AIMessageResponseDto> ClaimStickerAsync(Guid userId, Guid sessionId)
    {
        // 1) Tìm reward đang chờ (đọc nhanh, không tracking) đúng session
        var pendingRewardInfo = await dbContext.PendingStickerRewards
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.SessionId == sessionId && r.IsClaimed == false)
            .Select(r => new { r.RewardId, r.SessionId })
            .FirstOrDefaultAsync();

        if (pendingRewardInfo == null)
        {
            throw new NotFoundException("Không tìm thấy phần thưởng sticker nào đang chờ.", "REWARD_NOT_FOUND");
        }

        // 2) Acquire lock trên đúng session
        var lockAcquired = await concurrencyManager.TryAcquireSessionLockAsync(
            sessionId, TimeSpan.FromSeconds(15));

        if (!lockAcquired)
        {
            throw new TimeoutException("Hệ thống đang bận, không thể nhận thưởng lúc này. Vui lòng thử lại sau.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            // 3) Lấy reward (tracked) trong transaction để cập nhật
            var rewardToClaim = await dbContext.PendingStickerRewards
                .FirstOrDefaultAsync(r =>
                    r.RewardId == pendingRewardInfo.RewardId &&
                    r.IsClaimed == false &&
                    r.SessionId == sessionId);

            if (rewardToClaim == null)
            {
                // KHÔNG rollback ở đây; ném exception để finally/using tự xử lý.
                throw new NotFoundException("Sticker đã được nhận trong khi bạn chờ.", "REWARD_ALREADY_CLAIMED");
            }

            // 4) Kiểm tra session thuộc về user và active
            var targetSession = await dbContext.AIChatSessions
                .AsNoTracking()
                .AnyAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive == true);

            if (!targetSession)
            {
                throw new ForbiddenException("Không thể nhận thưởng cho session không tồn tại hoặc không thuộc về bạn.");
            }

            // 5) Gọi LLM (Emo) để tạo lời nhắn tặng sticker
            // (Có thể cân nhắc đưa ra ngoài transaction để rút ngắn thời gian giữ transaction.
            // Ở đây giữ nguyên theo luồng hiện tại.)
            string llmContext =
                $"[Bối cảnh hệ thống]: Emo, hãy tạo một tin nhắn để tặng người dùng sticker. Sticker này được tạo từ bối cảnh: '{rewardToClaim.PromptFilter}'. " +
                "Hãy giải thích ngắn gọn (1-2 câu) ý nghĩa của sticker này và gửi tặng họ. " +
                "Hãy tỏ ra vui vẻ và thấu hiểu. KHÔNG dùng markdown.";

            var aiPayload = new AIRequestPayload(
                Context: llmContext,
                Summarization: null, // IAIProvider sẽ tự điền
                HistoryMessages: []  // IAIProvider sẽ tự điền
            );

            var emoFollowUpText = await aiProvider.GenerateResponseAsync_FoundationalModel(aiPayload, sessionId);

            if (string.IsNullOrWhiteSpace(emoFollowUpText))
            {
                throw new Exception("Emo không thể tạo tin nhắn giới thiệu sticker lúc này.");
            }

            // 6) Tạo nội dung message dạng JSON (nhưng lưu/trả về dưới dạng string)
            var messageContentPayload = new
            {
                type = "reward_sticker",
                text = emoFollowUpText.Trim(),
                sticker_url = rewardToClaim.StickerUrl
            };

            // Giữ Unicode/emoji không escape
            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string jsonContent = JsonSerializer.Serialize(messageContentPayload, jsonOptions);

            
            // 7) Lấy BlockNumber tiếp theo
            int lastBlockNumber = await dbContext.AIChatMessages
                .Where(m => m.SessionId == sessionId)
                .MaxAsync(m => (int?)m.BlockNumber) ?? 0;

            // 8) Tạo message entity
            var newMessage = new AIMessage
            {
                Id = Guid.NewGuid(),
                SenderIsEmo = true,
                SenderUserId = null,
                SessionId = sessionId,
                Content = jsonContent, // Lưu JSON (string)
                CreatedDate = DateTimeOffset.UtcNow,
                IsRead = false,
                BlockNumber = lastBlockNumber + 1
            };

            // 9) Cập nhật DB
            rewardToClaim.Claim(); // đánh dấu đã nhận
            dbContext.PendingStickerRewards.Update(rewardToClaim);
            await dbContext.AIChatMessages.AddAsync(newMessage);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation(
                "User {UserId} claimed Reward {RewardId}. New message {MessageId} created in Session {SessionId}.",
                userId, rewardToClaim.RewardId, newMessage.Id, sessionId);

            // 10) Trả về DTO (giữ Content là string)
            return new AIMessageResponseDto(
                newMessage.SessionId,
                newMessage.SenderIsEmo,
                newMessage.Content,
                newMessage.CreatedDate
            );
        }
        catch (Exception ex)
        {
            // KHÔNG rollback ở đây — transaction sẽ tự rollback khi dispose nếu chưa commit.
            logger.LogError(ex, "Failed to claim sticker for user {UserId} in Session {SessionId}", userId, sessionId);
            throw;
        }
        finally
        {
            // Luôn giải phóng lock
            concurrencyManager.ReleaseSessionLock(sessionId);
        }
    }
}
