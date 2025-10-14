using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Domain.Aggregates.Reaction.DomainEvents;

public sealed record ReactionCreatedEvent(
    Guid ReactionId,
    ReactionTargetType TargetType,
    Guid TargetId,
    string ReactionCode,
    Guid ReactorAliasId
) : IDomainEvent;

public sealed record ReactionTypeChangedEvent(
    Guid ReactionId,
    ReactionTargetType TargetType,
    Guid TargetId,
    string OldReactionCode,
    string NewReactionCode,
    Guid EditorAliasId
) : IDomainEvent;

public sealed record ReactionRemovedEvent(
    Guid ReactionId,
    ReactionTargetType TargetType,
    Guid TargetId,
    string ReactionCode,
    Guid AuthorAliasId) : IDomainEvent;

public sealed record ReactionRestoredEvent(
    Guid ReactionId,
    ReactionTargetType TargetType,
    Guid TargetId,
    string ReactionCode,
    Guid AuthorAliasId) : IDomainEvent;

public sealed record ReactionAddedEvent(
    Guid ReactionId,
    string RequestTargetType,
    Guid RequestTargetId,
    string RequestReactionCode,
    Guid ActorResolverAliasId
) : IDomainEvent;

public record ReactionUpdatedEvent(
    Guid ExistingReactionId,
    string RequestTargetType,
    Guid RequestTargetId,
    string RequestReactionCode,
    Guid ActorResolverAliasId) : IDomainEvent;