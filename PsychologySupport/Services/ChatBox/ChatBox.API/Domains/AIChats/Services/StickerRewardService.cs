using System.Text.Json;
using BuildingBlocks.Exceptions;
using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Domains.AIChats.Services;

/// <summary>
/// Xử lý "Claim" sticker thưởng.
/// - Chỉ Commit khi thành công; exception sẽ khiến transaction tự rollback.
/// - Khóa theo sessionId để nhất quán.
/// - Tách rõ: đọc/validate → gọi LLM (ngoài transaction) → ghi DB trong transaction.
/// </summary>
public class StickerRewardService(
    ChatBoxDbContext dbContext,
    IAIProvider aiProvider,
    IAIRequestFactory aiRequestFactory,
    ISessionConcurrencyManager concurrencyManager,
    ILogger<StickerRewardService> logger) : IStickerRewardService
{
    public async Task<AIMessageResponseDto> ClaimStickerAsync(Guid userId, Guid sessionId)
    {
        // 1) Đọc thông tin reward pending (no-tracking)
        var pending = await GetPendingRewardOrThrowAsync(userId, sessionId);

        // 2) Acquire lock trên session để đảm bảo 1 claim tại 1 thời điểm
        var locked = await concurrencyManager.TryAcquireSessionLockAsync(sessionId, TimeSpan.FromSeconds(15));
        if (!locked)
            throw new TimeoutException("Hệ thống đang bận, không thể nhận thưởng lúc này. Vui lòng thử lại sau.");

        try
        {
            // 3) Validate session ownership (no-tracking)
            await EnsureSessionOwnedByUserAsync(userId, sessionId);

            // 4) Gọi LLM tạo lời nhắn follow-up (ngoài transaction để rút ngắn thời gian giữ tx)
            var emoFollowUpText = await GenerateStickerFollowupAsync(sessionId, pending.PromptFilter);
            if (string.IsNullOrWhiteSpace(emoFollowUpText))
                throw new Exception("Emo không thể tạo tin nhắn giới thiệu sticker lúc này.");

            // 5) Ghi DB trong transaction: đánh dấu claimed + tạo message
            var newMessage = await PersistClaimAsync(sessionId, pending.RewardId, emoFollowUpText.Trim());

            // 6) Trả DTO
            return new AIMessageResponseDto(
                newMessage.SessionId,
                newMessage.SenderIsEmo,
                newMessage.Content,
                newMessage.CreatedDate
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to claim sticker for user {UserId} in Session {SessionId}", userId, sessionId);
            throw;
        }
        finally
        {
            concurrencyManager.ReleaseSessionLock(sessionId);
        }
    }


    private async Task<(Guid RewardId, Guid SessionId, string PromptFilter, string StickerUrl)> GetPendingRewardOrThrowAsync(
        Guid userId, Guid sessionId)
    {
        var reward = await dbContext.PendingStickerRewards
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.SessionId == sessionId && !r.IsClaimed)
            .Select(r => new { r.RewardId, r.SessionId, r.PromptFilter, r.StickerUrl })
            .FirstOrDefaultAsync();

        if (reward is null)
            throw new NotFoundException("Không tìm thấy phần thưởng sticker nào đang chờ.", "REWARD_NOT_FOUND");

        return (reward.RewardId, reward.SessionId, reward.PromptFilter, reward.StickerUrl);
    }

    private async Task EnsureSessionOwnedByUserAsync(Guid userId, Guid sessionId)
    {
        var ok = await dbContext.AIChatSessions
            .AsNoTracking()
            .AnyAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive == true);

        if (!ok)
            throw new ForbiddenException("Không thể nhận thưởng cho session không tồn tại hoặc không thuộc về bạn.");
    }
    

    private async Task<string> GenerateStickerFollowupAsync(Guid sessionId, string promptFilter)
    {
        var llmContext =
            $"[NOW]: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}\n" +
            "[STICKER_BRIEF]\n" +
            $"- Chủ đề tạo hình: {promptFilter}\n" +
            "[/STICKER_BRIEF]";

        var payload = await aiRequestFactory.CreateAsync(
            historyMessages: [], 
            session: await dbContext.AIChatSessions.AsNoTracking().FirstAsync(s => s.Id == sessionId),
            augmentedContext: llmContext);

        var systemInstruction =
            "Bạn là Emo – bạn đồng hành thân thiện.\n   " +
            " Nhiệm vụ: viết 1–2 câu ngắn (≤40 từ, tiếng Việt) để tặng sticker dựa trên STICKER_BRIEF và ngữ cảnh gần nhất.\n    " +
            "Giọng cậu–tớ, ấm, tự nhiên, chân thành. Chỉ trả về thuần văn bản: không markdown, không liệt kê, không emoji, không dạy đời, không hứa hẹn “chữa lành”.\n    " +
            "Không bịa chi tiết ngoài brief. Ưu tiên gợi tả hình ảnh hoặc ý nghĩa ngắn gọn liên quan chủ đề sticker và cảm xúc vừa chia sẻ.\n    " +
            "Kết thúc có thể gợi mở 1 câu hỏi rất nhẹ để cậu phản hồi (ví dụ: “cậu thấy chi tiết này có đúng tâm trạng không?”).\n    " +
            "Nếu gặp nội dung nhạy cảm (tự hại/bạo lực/lạm dụng), chỉ bày tỏ quan tâm ngắn gọn và khuyến khích tìm hỗ trợ tin cậy; tuyệt đối không hướng dẫn hay chẩn đoán.";
        
        return await aiProvider.GenerateChatResponseAsync(
            payload, systemInstruction);
    }

    private async Task<AIMessage> PersistClaimAsync(Guid sessionId, Guid rewardId, string followupText)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync();

        var rewardToClaim = await dbContext.PendingStickerRewards
            .FirstOrDefaultAsync(r => r.RewardId == rewardId && r.SessionId == sessionId && !r.IsClaimed);

        if (rewardToClaim is null)
            throw new NotFoundException("Sticker đã được nhận trong khi bạn chờ.", "REWARD_ALREADY_CLAIMED");

        var contentJson = BuildStickerMessageContent(followupText, rewardToClaim.StickerUrl);

        var lastBlock = await dbContext.AIChatMessages
            .Where(m => m.SessionId == sessionId)
            .MaxAsync(m => (int?)m.BlockNumber) ?? 0;

        var message = new AIMessage
        {
            Id = Guid.NewGuid(),
            SenderIsEmo = true,
            SenderUserId = null,
            SessionId = sessionId,
            Content = contentJson,
            CreatedDate = DateTimeOffset.UtcNow,
            IsRead = false,
            BlockNumber = lastBlock + 1
        };

        rewardToClaim.Claim();
        dbContext.PendingStickerRewards.Update(rewardToClaim);
        await dbContext.AIChatMessages.AddAsync(message);

        await dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        logger.LogInformation("Claimed Reward {RewardId}. New message {MessageId} created in Session {SessionId}.",
            rewardId, message.Id, sessionId);

        return message;
    }

    private static string BuildStickerMessageContent(string text, string stickerUrl)
    {
        var payload = new
        {
            type = "reward_sticker",
            text,
            sticker_url = stickerUrl
        };

        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return JsonSerializer.Serialize(payload, options);
    }
}