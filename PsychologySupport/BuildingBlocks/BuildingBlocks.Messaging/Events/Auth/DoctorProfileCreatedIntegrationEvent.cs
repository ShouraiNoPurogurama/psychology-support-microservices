using BuildingBlocks.Data.Enums;

namespace BuildingBlocks.Messaging.Events.Auth
{
    public record DoctorProfileCreatedIntegrationEvent(
        Guid UserId,
        UserGender Gender,
        string Email,
        string PhoneNumber,
        DateTimeOffset? CreatedAt
    ) : IntegrationEvents;
}
