using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Profile
{
    //TODO tí quay lại check
    public record PatientProfileUpdatedIntegrationEvent(
        // Guid UserId,
        string FullName,
        UserGender Gender,
        string Email,
        string PhoneNumber
    ) : IntegrationEvent;
}
