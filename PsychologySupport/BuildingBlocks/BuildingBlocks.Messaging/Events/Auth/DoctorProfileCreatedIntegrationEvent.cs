using BuildingBlocks.Data.Enums;
using BuildingBlocks.Messaging.Events.Notification;

namespace BuildingBlocks.Messaging.Events.Auth
{
    public record DoctorProfileCreatedIntegrationEvent(
        Guid UserId,
        string FullName,
        UserGender Gender,
        string Email,
        string PhoneNumber
    ) : SendEmailIntegrationEvent(Email, "Welcome to Psychology Support", "Hello world");
}
