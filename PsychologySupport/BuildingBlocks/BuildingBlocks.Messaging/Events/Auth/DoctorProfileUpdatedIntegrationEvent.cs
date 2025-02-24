using BuildingBlocks.Data.Enums;

namespace BuildingBlocks.Messaging.Events.Auth;

public record DoctorProfileUpdatedIntegrationEvent(
    Guid UserId,
    UserGender Gender,
    string Email,
    string PhoneNumber,
    DateTimeOffset? LastModified
);
