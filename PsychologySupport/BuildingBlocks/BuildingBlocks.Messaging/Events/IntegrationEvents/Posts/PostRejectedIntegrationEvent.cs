namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostRejectedIntegrationEvent(
    Guid PostId,
    Guid AuthorAliasId,
    Guid ModeratorAliasId,
    string Reason,
    DateTimeOffset RejectedAt
) : IntegrationEvent;
