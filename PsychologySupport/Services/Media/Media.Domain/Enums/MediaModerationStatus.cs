using System.Text.Json.Serialization;

namespace Media.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MediaModerationStatus
    {
        /// <summary>
        /// Chưa được kiểm duyệt hoặc đang chờ kiểm duyệt.
        /// </summary>
        Pending,

        /// <summary>
        /// Kết quả: Nội dung được chấp thuận.
        /// </summary>
        Approved,

        /// <summary>
        /// Kết quả: Nội dung bị từ chối do vi phạm.
        /// </summary>
        Rejected,
        
        /// <summary>
        /// Kết quả: Hệ thống tự động không chắc chắn, cần người xem xét.
        /// </summary>
        Flagged
    }
}