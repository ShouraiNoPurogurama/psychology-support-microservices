namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;

public record AliasFollowedIntegrationEvent(
    Guid FollowerAliasId,
    Guid FollowedAliasId,
    DateTimeOffset FollowedAt
) : IntegrationEvent;