namespace Post.Domain.Events;

// Comment events
public sealed record CommentCreatedEvent(Guid CommentId, Guid PostId, Guid? ParentCommentId, Guid AuthorAliasId)
    : DomainEvent(CommentId);

public sealed record CommentContentUpdatedEvent(Guid CommentId, string OldContent, string NewContent) : DomainEvent(CommentId);

public sealed record CommentApprovedEvent(Guid CommentId, Guid ModeratorAliasId) : DomainEvent(CommentId);

public sealed record CommentRejectedEvent(Guid CommentId, List<string> Reasons, Guid ModeratorAliasId) : DomainEvent(CommentId);

public sealed record CommentReactionCountChangedEvent(Guid CommentId, int Delta) : DomainEvent(CommentId);

public sealed record CommentReplyCountChangedEvent(Guid CommentId, int Delta) : DomainEvent(CommentId);

public sealed record CommentDeletedEvent(Guid CommentId, Guid PostId, Guid DeleterAliasId) : DomainEvent(CommentId);

public sealed record CommentRestoredEvent(Guid CommentId, Guid RestorerAliasId) : DomainEvent(CommentId);