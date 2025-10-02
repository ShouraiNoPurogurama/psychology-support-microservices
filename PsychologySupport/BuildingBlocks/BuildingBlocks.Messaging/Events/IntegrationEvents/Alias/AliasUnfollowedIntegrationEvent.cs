namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;

public record AliasUnfollowedIntegrationEvent(
    Guid FollowerAliasId,
    Guid UnfollowedAliasId,
    DateTimeOffset UnfollowedAt
) : IntegrationEvent;
