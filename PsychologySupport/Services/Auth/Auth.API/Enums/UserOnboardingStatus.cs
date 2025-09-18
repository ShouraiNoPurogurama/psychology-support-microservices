using System.Text.Json.Serialization;

namespace Auth.API.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserOnboardingStatus
{
    Pending = 0,     //Đăng ký thành công nhưng chưa có profile
    Completed = 1,      //Hồ sơ hoàn tất, có thể login full quyền
    NeedsAction = 2 //Lỗi hoặc thiếu dữ liệu, buộc user hoàn tất hồ sơ
}