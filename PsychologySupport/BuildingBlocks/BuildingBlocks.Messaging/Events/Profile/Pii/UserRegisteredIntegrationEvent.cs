namespace BuildingBlocks.Messaging.Events.Profile.Pii;

public record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string? PhoneNumber,
    string Address,
    string FullName,
    DateOnly BirthDate,
    Enums.UserGender Gender
) : IntegrationEvents;