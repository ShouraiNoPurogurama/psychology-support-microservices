using System.Text.Json.Serialization;

namespace Media.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MediaState
    {
        // --- Giai đoạn đầu vào & Kiểm duyệt ---

        /// <summary>
        /// 1. Mới tải lên, đang chờ được đưa vào hàng đợi kiểm duyệt.
        /// </summary>
        Pending,

        /// <summary>
        /// 2. Đang trong quá trình kiểm duyệt nội dung tự động trên file gốc.
        /// Đây là cổng kiểm soát đầu tiên.
        /// </summary>
        Moderating,

        /// <summary>
        /// Cần sự xem xét của người thật sau khi kiểm duyệt tự động.
        /// </summary>
        RequiresManualReview,

        // --- Giai đoạn Xử lý Kỹ thuật ---

        /// <summary>
        /// 3. Đã qua kiểm duyệt, đang trong quá trình xử lý kỹ thuật (tạo thumbnail, transcode...).
        /// </summary>
        Processing,

        // --- Giai đoạn Sẵn sàng & Sử dụng ---

        /// <summary>
        /// 4. Sẵn sàng để được gắn kết và hiển thị, đã qua tất cả các bước.
        /// </summary>
        Ready,

        /// <summary>
        /// 5. Đã được gắn kết vào một thực thể (Post, Comment, Profile...).
        /// </summary>
        Attached,

        // --- Các trạng thái kết thúc hoặc bị hạn chế ---

        /// <summary>
        /// Tạm ẩn bởi người dùng hoặc quản trị viên. Có thể xảy ra ở bất kỳ giai đoạn nào sau khi sẵn sàng.
        /// </summary>
        Hidden,

        /// <summary>
        /// Bị chặn hiển thị vĩnh viễn do vi phạm chính sách nội dung.
        /// </summary>
        Blocked,

        /// <summary>
        /// Đã bị xóa (soft delete). Trạng thái cuối cùng.
        /// </summary>
        Deleted
    }
}