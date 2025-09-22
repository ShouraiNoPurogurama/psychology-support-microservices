namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;

public record UserUnfollowedIntegrationEvent(
    Guid UnfollowerAliasId,
    Guid UnfollowedAliasId
) : IntegrationEvent;

