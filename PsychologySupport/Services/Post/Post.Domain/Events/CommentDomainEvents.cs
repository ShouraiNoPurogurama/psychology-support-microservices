namespace Post.Domain.Events;

// Comment Domain Events
public record CommentCreatedEvent(Guid CommentId, Guid PostId, Guid? ParentCommentId, Guid AuthorAliasId) : IDomainEvent;
public record CommentContentUpdatedEvent(Guid CommentId, string OldContent, string NewContent) : IDomainEvent;
public record CommentApprovedEvent(Guid CommentId, Guid ModeratorId) : IDomainEvent;
public record CommentRejectedEvent(Guid CommentId, IReadOnlyList<string> Reasons, Guid ModeratorId) : IDomainEvent;
public record CommentReactionCountChangedEvent(Guid CommentId, int Delta) : IDomainEvent;
public record CommentReplyCountChangedEvent(Guid CommentId, int Delta) : IDomainEvent;
public record CommentDeletedEvent(Guid CommentId, Guid PostId, Guid DeleterAliasId) : IDomainEvent;
public record CommentRestoredEvent(Guid CommentId, Guid RestorerAliasId) : IDomainEvent;
