using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.Gift.ValueObjects;

/// <summary>
/// Mô tả mục tiêu nhận quà (post/comment).
/// </summary>
public sealed record GiftTarget
{
    public string TargetType { get; init; } = default!;
    public Guid TargetId { get; init; }

    // Cho EF Core materialization
    private GiftTarget() { }

    // Ctor kín – chỉ factory được phép gọi
    private GiftTarget(string targetType, Guid targetId)
    {
        TargetType = targetType;
        TargetId = targetId;
    }

    /// <summary>
    /// Tạo GiftTarget. Chỉ chấp nhận "post" hoặc "comment".
    /// </summary>
    public static GiftTarget Create(string targetType, Guid targetId)
    {
        if (string.IsNullOrWhiteSpace(targetType))
            throw new InvalidGiftDataException("Loại mục tiêu nhận quà không được để trống.");

        if (targetId == Guid.Empty)
            throw new InvalidGiftDataException("ID mục tiêu nhận quà không hợp lệ.");

        var normalized = targetType.Trim().ToLowerInvariant();
        var valid = new[] { "post", "comment" };
        if (!valid.Contains(normalized))
            throw new InvalidGiftDataException($"Loại mục tiêu nhận quà không hợp lệ. Chỉ chấp nhận: {string.Join(", ", valid)}.");

        return new GiftTarget(normalized, targetId);
    }

    public bool IsPost => TargetType == "post";
    public bool IsComment => TargetType == "comment";
}

/// <summary>
/// Thông tin quà tặng (định danh loại quà).
/// </summary>
public sealed record GiftInfo
{
    public Guid GiftId { get; init; }

    // Cho EF Core materialization
    private GiftInfo() { }

    // Ctor kín – chỉ factory được phép gọi
    private GiftInfo(Guid giftId)
    {
        GiftId = giftId;
    }

    /// <summary>
    /// Tạo GiftInfo từ ID quà tặng.
    /// </summary>
    public static GiftInfo Create(Guid giftId)
    {
        if (giftId == Guid.Empty)
            throw new InvalidGiftDataException("ID quà tặng không hợp lệ.");

        return new GiftInfo(giftId);
    }
}