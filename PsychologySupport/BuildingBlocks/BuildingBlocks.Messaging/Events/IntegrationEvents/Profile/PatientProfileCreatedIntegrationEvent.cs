using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;

namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Profile
{
    //TODO tí quay lại sửa
    public record PatientProfileCreatedIntegrationEvent(
        // Guid UserId,
        string FullName,
        UserGender Gender,
        string PhoneNumber,
        string Email,
        string Subject,
        string Body
    ) : SendEmailIntegrationEvent(Email, Subject, Body); 
}
