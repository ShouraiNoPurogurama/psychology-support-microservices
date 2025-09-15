namespace Post.Domain.Events;

public sealed record CategoryTagCreatedEvent(Guid CategoryTagId, string Code, string DisplayName) : DomainEvent(CategoryTagId);

public sealed record CategoryTagUpdatedEvent(Guid CategoryTagId, string Code, string OldDisplayName, string NewDisplayName)
    : DomainEvent(CategoryTagId);

public sealed record CategoryTagStatusChangedEvent(Guid CategoryTagId, string Code, bool IsActive) : DomainEvent(CategoryTagId);