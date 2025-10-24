namespace Post.Domain.Aggregates.Gifts.DomainEvents;

public sealed record GiftAttachedEvent(
    Guid PostId,
    Guid PostAuthorAliasId,
    Guid SenderAliasId,
    Guid GiftId,
    decimal Amount,
    string? Message,
    DateTimeOffset SentAt
) : DomainEvent;