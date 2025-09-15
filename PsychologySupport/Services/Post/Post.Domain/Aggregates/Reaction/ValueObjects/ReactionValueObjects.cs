using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.Reaction.ValueObjects;

public sealed record ReactionTarget
{
    public string TargetType { get; init; } = default!;
    public Guid TargetId { get; init; }

    // EF Core
    private ReactionTarget() { }

    private ReactionTarget(string targetType, Guid targetId)
    {
        TargetType = targetType;
        TargetId = targetId;
    }

    /// <summary>
    /// Tạo ReactionTarget. TargetType hợp lệ: "post", "comment".
    /// </summary>
    public static ReactionTarget Create(string targetType, Guid targetId)
    {
        if (string.IsNullOrWhiteSpace(targetType))
            throw new InvalidReactionDataException("Loại mục tiêu tương tác không được để trống.");

        if (targetId == Guid.Empty)
            throw new InvalidReactionDataException("ID mục tiêu tương tác không hợp lệ.");

        var normalized = targetType.Trim().ToLowerInvariant();
        // Nếu sau này có thêm "reply" hoặc "message" chỉ cần mở rộng danh sách này:
        var valid = new[] { "post", "comment" };
        if (!valid.Contains(normalized))
            throw new InvalidReactionDataException($"Loại mục tiêu tương tác không hợp lệ. Chỉ chấp nhận: {string.Join(", ", valid)}.");

        return new ReactionTarget(normalized, targetId);
    }

    public bool IsPost => TargetType == "post";
    public bool IsComment => TargetType == "comment";
}


/// <summary>
/// VO mô tả loại tương tác. Không tự truy vấn DB.
/// Metadata (emoji/icon, weight, enabled) phải được cung cấp từ lớp trên (Application).
/// </summary>
public sealed record ReactionType
{
    public string Code { get; init; } = default!;   // ví dụ: "like", "love", "care", "haha"
    public string? Emoji { get; init; }             // hiển thị UI: có thể là emoji hoặc icon key
    public int Weight { get; init; }                // dùng cho ranking nếu cần

    // EF Core
    private ReactionType() { }

    private ReactionType(string code, string? emoji, int weight)
    {
        Code = code;
        Emoji = emoji;
        Weight = weight;
    }

    /// <summary>
    /// Factory: nhận code + metadata đã được load/validate ở Application layer.
    /// </summary>
    public static ReactionType Create(string code, string? emoji, int weight, bool isEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidReactionDataException("Mã loại tương tác không được để trống.");

        var normalized = code.Trim().ToLowerInvariant();

        if (!isEnabled)
            throw new InvalidReactionDataException($"Loại tương tác '{normalized}' đang bị vô hiệu hoá.");

        //weight tối thiểu 0 để tránh tác động xấu ranking
        var safeWeight = Math.Max(0, weight);

        return new ReactionType(normalized, emoji, safeWeight);
    }

    // Gợi ý heuristic nếu cần:
    public bool IsPositive => Code is "like" or "love" or "care" or "celebrate";
    public bool IsNegative => Code is "sad" or "angry";
    public bool IsNeutral  => !(IsPositive || IsNegative);
    public bool IsHighWeight => Weight > 1;
}
