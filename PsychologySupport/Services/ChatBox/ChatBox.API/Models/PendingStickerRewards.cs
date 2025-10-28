using BuildingBlocks.DDD;

namespace ChatBox.API.Models;

public class PendingStickerReward : IHasCreationAudit
{
    /// <summary>
    /// PK. Lấy từ RewardId của UserMemory service.
    /// </summary>
    public Guid RewardId { get; private set; }

    /// <summary>
    /// ID của người dùng (UserId)
    /// </summary>
    public Guid UserId { get; private set; }
    
    public Guid SessionId { get; private set; }

    /// <summary>
    /// URL của sticker đã được tạo
    /// </summary>
    public string StickerUrl { get; private set; }

    /// <summary>
    /// Context (prompt) đã dùng để tạo ảnh,
    /// dùng để Emo sinh câu chat tiếp lời.
    /// </summary>
    public string PromptFilter { get; private set; }

    /// <summary>
    /// Đánh dấu sticker này đã được claim (gửi cho user) hay chưa.
    /// </summary>
    public bool IsClaimed { get; private set; }

    /// <summary>
    /// Thời điểm record này được tạo (khi nhận event).
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    /// <summary>
    /// Thời điểm người dùng gọi API claim.
    /// </summary>
    public DateTimeOffset? ClaimedAt { get; private set; }

    private PendingStickerReward()
    {
    }

    // Constructor khi tạo mới (lúc nhận event)
    public PendingStickerReward(Guid rewardId, Guid userId, Guid sessionId, string stickerUrl, string promptFilter)
    {
        RewardId = rewardId;
        UserId = userId;
        SessionId = sessionId;
        StickerUrl = stickerUrl;
        PromptFilter = promptFilter;
        IsClaimed = false;
    }

    // Method nghiệp vụ để "claim"
    public void Claim()
    {
        if (IsClaimed)
        {
            // Có thể bắn exception nếu nghiệp vụ yêu cầu
            // throw new InvalidOperationException("Sticker has already been claimed.");
            return; // Hoặc im lặng
        }

        IsClaimed = true;
        ClaimedAt = DateTimeOffset.UtcNow;
    }
}