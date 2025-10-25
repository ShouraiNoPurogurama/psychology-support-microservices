namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;

public record UserRegisteredIntegrationEvent(
    Guid SeedPatientProfileId,
    Guid SeedSubjectRef,
    Guid UserId,
    string Email,
    string? PhoneNumber,
    string FullName,
    string Role = "User",
    string? ReferralCode = null
) : IntegrationEvent;