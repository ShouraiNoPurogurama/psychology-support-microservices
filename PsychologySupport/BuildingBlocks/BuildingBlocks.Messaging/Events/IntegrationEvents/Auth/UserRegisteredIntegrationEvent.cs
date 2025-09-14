namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;

public record UserRegisteredIntegrationEvent(
    Guid SeedProfileId,
    Guid SeedSubjectRef,
    Guid UserId,
    string Email,
    string? PhoneNumber,
    string FullName,
    string Role = "User"
) : IntegrationEvent;