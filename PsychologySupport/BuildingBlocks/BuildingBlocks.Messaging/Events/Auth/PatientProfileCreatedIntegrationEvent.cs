using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Notification;

namespace BuildingBlocks.Messaging.Events.Auth
{
    public record PatientProfileCreatedIntegrationEvent(
        Guid UserId,
        string FullName,
        UserGender Gender,
        string PhoneNumber,
        string Email,
        string Subject,
        string Body
    ) : SendEmailIntegrationEvent(Email, Subject, Body); 
}
