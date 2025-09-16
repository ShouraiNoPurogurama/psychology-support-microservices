namespace Post.Domain.Aggregates.Reaction.DomainEvents;

public sealed record ReactionCreatedEvent(
    Guid ReactionId,
    string TargetType,
    Guid TargetId,
    string ReactionCode,
    Guid AuthorAliasId
) : DomainEvent;

public sealed record ReactionTypeChangedEvent(
    Guid ReactionId,
    string TargetType,
    Guid TargetId,
    string OldReactionCode,
    string NewReactionCode,
    Guid EditorAliasId
) : DomainEvent;

public sealed record ReactionRemovedEvent(
    Guid ReactionId,
    string TargetType,
    Guid TargetId,
    string ReactionCode,
    Guid AuthorAliasId) : DomainEvent(ReactionId);

public sealed record ReactionRestoredEvent(
    Guid ReactionId,
    string TargetType,
    Guid TargetId,
    string ReactionCode,
    Guid AuthorAliasId) : DomainEvent(ReactionId);

public sealed record ReactionAddedEvent(
    Guid ReactionId,
    string RequestTargetType,
    Guid RequestTargetId,
    string RequestReactionCode,
    Guid ActorResolverAliasId
) : DomainEvent(ReactionId);

public record ReactionUpdatedEvent(
    Guid ExistingReactionId,
    string RequestTargetType,
    Guid RequestTargetId,
    string RequestReactionCode,
    Guid ActorResolverAliasId) : DomainEvent;