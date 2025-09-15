namespace Post.Domain.Events;

// Reaction Domain Events
public record ReactionCreatedEvent(Guid ReactionId, string TargetType, Guid TargetId, string ReactionType, Guid AuthorAliasId) : IDomainEvent;
public record ReactionTypeChangedEvent(Guid ReactionId, string TargetType, Guid TargetId, string OldType, string NewType) : IDomainEvent;
public record ReactionRemovedEvent(Guid ReactionId, string TargetType, Guid TargetId, string ReactionType, Guid RemoverAliasId) : IDomainEvent;
public record ReactionRestoredEvent(Guid ReactionId, string TargetType, Guid TargetId, string ReactionType, Guid RestorerAliasId) : IDomainEvent;
