namespace Post.Domain.Aggregates.Posts.Enums;

public enum PostStatus
{
    /// <summary>
    /// Đang trong quá trình khởi tạo, chờ các service khác xác nhận (vd: Media).
    /// </summary>
    Creating,

    /// <summary>
    /// Đã khởi tạo thành công và sẵn sàng để sử dụng.
    /// </summary>
    Finalized,

    /// <summary>
    /// Quá trình khởi tạo thất bại.
    /// </summary>
    CreationFailed
}