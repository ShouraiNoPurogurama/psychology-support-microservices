namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record GiftAttachedIntegrationEvent(
    Guid PostId,
    Guid PostAuthorAliasId,
    Guid SenderAliasId,
    string SenderAliasLabel,
    Guid GiftId,
    decimal Amount,
    string? Message,
    DateTimeOffset SentAt
    ) : IntegrationEvent;