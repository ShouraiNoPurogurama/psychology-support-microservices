using System.Text.Json.Serialization;

namespace Media.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MediaState
    {
        /// <summary>
        /// Mới tải lên, chờ hệ thống xử lý.
        /// </summary>
        Pending,

        /// <summary>
        /// Đang xử lý kỹ thuật (transcode, etc.) HOẶC đang chờ kết quả kiểm duyệt nội dung.
        /// </summary>
        Processing,
        
        /// <summary>
        /// Đang trong quá trình kiểm duyệt nội dung (tự động).
        /// </summary>
        Moderating,

        /// <summary>
        /// Cần sự xem xét của người thật sau khi kiểm duyệt tự động.
        /// </summary>
        RequiresManualReview,

        /// <summary>
        /// Sẵn sàng để hiển thị, đã qua tất cả các bước kiểm tra.
        /// </summary>
        Ready,

        /// <summary>
        /// Bị chặn hiển thị do vi phạm chính sách nội dung.
        /// </summary>
        Blocked,

        /// <summary>
        /// Tạm ẩn bởi người dùng hoặc quản trị viên.
        /// </summary>
        Hidden,

        /// <summary>
        /// Đã bị xóa (soft delete). Trạng thái cuối cùng.
        /// </summary>
        Deleted
    }
}