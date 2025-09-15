namespace Post.Domain.Events;

public sealed record GiftAttachedEvent(
    Guid PostId,
    Guid GiftId,
    Guid SenderAliasId,
    decimal Amount,
    string? Message
) : DomainEvent;