namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;

public record UserBlockedIntegrationEvent(
    Guid BlockerAliasId,
    Guid BlockedAliasId
) : IntegrationEvent;

