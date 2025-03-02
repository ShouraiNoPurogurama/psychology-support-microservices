using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Auth;

public record DoctorProfileUpdatedIntegrationEvent(
    Guid UserId,
    string FullName,
    UserGender Gender,
    string Email,
    string PhoneNumber,
    DateTimeOffset? LastModified
);
