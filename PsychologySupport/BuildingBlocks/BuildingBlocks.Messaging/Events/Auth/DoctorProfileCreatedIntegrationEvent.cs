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
            "Welcome to Psychology Support",
            $"Dear {FullName},\n\nYour account has been created.\n\nLogin: {Email}\nPassword: {Password}\n\nPlease change your password after logging in."
    );
}
