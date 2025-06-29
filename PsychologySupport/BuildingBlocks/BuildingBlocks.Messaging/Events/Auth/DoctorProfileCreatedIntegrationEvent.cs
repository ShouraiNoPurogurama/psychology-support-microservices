using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Notification;

namespace BuildingBlocks.Messaging.Events.Auth
{
  /*  public record DoctorProfileCreatedIntegrationEvent(
        Guid UserId,
        string FullName,
        UserGender Gender,
        string Email,
        string PhoneNumber
    ) : SendEmailIntegrationEvent(Email, "Welcome to Psychology Support", "Hello world");
*/
    public record DoctorProfileCreatedIntegrationEvent(
        string FullName,
        UserGender Gender,
        string Email,
        string PhoneNumber,
        string Password
        ) : SendEmailIntegrationEvent(
            Email,
            "Chào mừng bạn đến với EmoEase",
            $"Xin chào {FullName},\n\nTài khoản của bạn đã được tạo thành công.\n\nTên đăng nhập: {Email}\nMật khẩu: {Password}\n\nVui lòng đổi mật khẩu sau khi đăng nhập."
    );
}
