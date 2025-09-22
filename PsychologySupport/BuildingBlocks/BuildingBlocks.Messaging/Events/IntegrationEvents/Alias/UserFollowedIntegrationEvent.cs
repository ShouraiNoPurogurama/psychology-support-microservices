namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;

public record UserFollowedIntegrationEvent(
    Guid FollowerAliasId,
    Guid FollowedAliasId,
    DateTimeOffset Timestamp
) : IntegrationEvent;

