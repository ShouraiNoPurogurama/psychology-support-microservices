namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Auth;

public record TokenRevokedIntegrationEvent(
    string Jti, 
    DateTimeOffset ExpirationUtc
) : IntegrationEvent;