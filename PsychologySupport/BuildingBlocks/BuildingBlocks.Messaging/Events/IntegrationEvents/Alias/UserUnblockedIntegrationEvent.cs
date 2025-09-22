namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;

public record UserUnblockedIntegrationEvent(
    Guid UnblockerAliasId,
    Guid UnblockedAliasId
) : IntegrationEvent;

