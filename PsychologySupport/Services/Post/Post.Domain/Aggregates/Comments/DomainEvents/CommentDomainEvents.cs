namespace Post.Domain.Aggregates.Comments.DomainEvents;

// Comment events
public sealed record CommentCreatedEvent(Guid CommentId, string CommentContent, Guid PostId, Guid? ParentCommentId, Guid AuthorAliasId)
    : IDomainEvent;

public sealed record CommentContentUpdatedEvent(Guid CommentId, string OldContent, string NewContent) : IDomainEvent;

public sealed record CommentApprovedEvent(Guid CommentId, Guid ModeratorAliasId) : IDomainEvent;

public sealed record CommentRejectedEvent(Guid CommentId, List<string> Reasons, Guid ModeratorAliasId) : IDomainEvent;

public sealed record CommentReactionCountChangedEvent(Guid CommentId, int Delta) : IDomainEvent;

public sealed record CommentReplyCountChangedEvent(Guid CommentId, int Delta) : IDomainEvent;

public sealed record CommentDeletedEvent(Guid CommentId, Guid PostId, Guid DeleterAliasId) : IDomainEvent;

public sealed record CommentRestoredEvent(Guid CommentId, Guid RestorerAliasId) : IDomainEvent;