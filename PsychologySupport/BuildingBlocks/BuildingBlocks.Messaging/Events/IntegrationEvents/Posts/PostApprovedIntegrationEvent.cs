namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostApprovedIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId,
    Guid ModeratorAliasId,
    DateTimeOffset ApprovedAt
) : IntegrationEvent;
