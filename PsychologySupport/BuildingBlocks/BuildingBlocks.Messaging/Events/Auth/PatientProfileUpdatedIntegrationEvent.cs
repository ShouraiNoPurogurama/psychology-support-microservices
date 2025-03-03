using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Auth
{
    public record PatientProfileUpdatedIntegrationEvent(
        Guid UserId,
        string FullName,
        UserGender Gender,
        string Email,
        string PhoneNumber
    ) : IntegrationEvents;
}
