using Post.Domain.Enums;

namespace Post.Domain.Events;

public sealed record PostCreatedEvent(
    Guid PostId,
    Guid AuthorAliasId,
    string Content,
    string? Title,
    string Visibility,
    List<string> CategoryCodes,
    DateTimeOffset CreatedAt
) : DomainEvent(PostId);

public sealed record PostUpdatedEvent(Guid PostId, Guid AuthorAliasId) : DomainEvent(PostId);

public sealed record PostDeletedEvent(Guid PostId, Guid AuthorAliasId) : DomainEvent(PostId);

public sealed record PostRestoredEvent(Guid PostId, Guid RestorerAliasId) : DomainEvent(PostId);

public sealed record PostApprovedEvent(Guid PostId, Guid ModeratorAliasId) : DomainEvent(PostId);

public sealed record PostRejectedEvent(Guid PostId, List<string> Reasons, Guid ModeratorAliasId) : DomainEvent(PostId);

public sealed record PostMediaAddedEvent(Guid PostId, Guid MediaId) : DomainEvent(PostId);

public sealed record PostMediaRemovedEvent(Guid PostId, Guid MediaId) : DomainEvent(PostId);

public sealed record PostCategoryAddedEvent(Guid PostId, Guid CategoryTagId) : DomainEvent(PostId);

public sealed record PostCategoryRemovedEvent(Guid PostId, Guid CategoryTagId) : DomainEvent(PostId);

public sealed record PostMetricsUpdatedEvent(Guid PostId, string CounterType, int Delta) : DomainEvent(PostId);

public sealed record PostViewedEvent(Guid PostId) : DomainEvent(PostId);

public sealed record PostVisibilityChangedEvent(Guid Id, PostVisibility OldVisibility, PostVisibility NewVisibility)
    : DomainEvent(Id);

public sealed record PostAbandonedEvent(Guid PostId, Guid AuthorAliasId, DateTimeOffset PostCreatedAt, DateTimeOffset AbandonedAt)
    : DomainEvent(PostId);

public sealed record PostContentUpdatedEvent(Guid PostId, string OldContent, string NewContent, Guid EditorAliasId)
    : DomainEvent(PostId);