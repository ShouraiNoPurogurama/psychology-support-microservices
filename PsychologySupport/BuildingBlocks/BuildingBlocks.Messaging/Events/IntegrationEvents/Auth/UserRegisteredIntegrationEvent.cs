namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;

public record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string? PhoneNumber,
    string Address,
    string FullName,
    DateOnly BirthDate,
    Enums.UserGender Gender
) : IntegrationEvent;