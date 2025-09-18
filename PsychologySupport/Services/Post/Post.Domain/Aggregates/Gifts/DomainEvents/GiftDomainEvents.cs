namespace Post.Domain.Aggregates.Gifts.DomainEvents;

public sealed record GiftAttachedEvent(
    Guid PostId,
    Guid GiftId,
    Guid SenderAliasId,
    decimal Amount,
    string? Message
) : DomainEvent;