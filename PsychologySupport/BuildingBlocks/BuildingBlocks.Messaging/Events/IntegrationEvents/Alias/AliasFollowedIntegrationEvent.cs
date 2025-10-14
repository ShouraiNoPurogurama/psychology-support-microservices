namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Alias;

public record AliasFollowedIntegrationEvent(
    Guid FollowerAliasId,
    Guid FollowedAliasId,
    string FollowerAliasLabel,
    DateTimeOffset FollowedAt
) : IntegrationEvent;