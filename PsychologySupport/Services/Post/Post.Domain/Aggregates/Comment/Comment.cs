using Post.Domain.Aggregates.Comment.ValueObjects;
using Post.Domain.Aggregates.Post.ValueObjects;
using Post.Domain.Events;
using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.Comment;

public sealed class Comment : AggregateRoot<Guid>, ISoftDeletable
{
    // Value Objects
    public CommentContent Content { get; private set; } = null!;
    public CommentHierarchy Hierarchy { get; private set; } = null!;
    public AuthorInfo Author { get; private set; } = null!;
    public ModerationInfo Moderation { get; private set; } = null!;

    // Properties
    public Guid PostId { get; private set; }
    public DateTime EditedAt { get; private set; }
    public int ReactionCount { get; private set; }
    public int ReplyCount { get; private set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

    // EF Core materialize
    private Comment() { }

    /// <summary>
    /// Factory method to create a new comment
    /// </summary>
    public static Comment Create(
        Guid postId,
        Guid authorAliasId,
        string content,
        Guid authorAliasVersionId,
        Guid? parentCommentId = null,
        string? parentPath = null)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            Content = CommentContent.Create(content),
            Hierarchy = CommentHierarchy.Create(parentCommentId, parentPath),
            Author = AuthorInfo.Create(authorAliasId, (Guid?)authorAliasVersionId),
            Moderation = ModerationInfo.Pending(),
            ReactionCount = 0,
            ReplyCount = 0
        };

        comment.AddDomainEvent(new CommentCreatedEvent(comment.Id, postId, parentCommentId, authorAliasId));
        return comment;
    }

    /// <summary>
    /// Update comment content
    /// </summary>
    public void UpdateContent(string newContent, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        var oldContent = Content.Value;
        Content = CommentContent.Create(newContent);
        EditedAt = DateTime.UtcNow;

        AddDomainEvent(new CommentContentUpdatedEvent(Id, oldContent, Content.Value));
    }

    /// <summary>
    /// Approve comment for display
    /// </summary>
    public void Approve(string policyVersion, Guid moderatorId)
    {
        Moderation = Moderation.Approve(policyVersion);
        AddDomainEvent(new CommentApprovedEvent(Id, moderatorId));
    }

    /// <summary>
    /// Reject comment with reasons
    /// </summary>
    public void Reject(List<string> reasons, string policyVersion, Guid moderatorId)
    {
        Moderation = Moderation.Reject(reasons, policyVersion);
        AddDomainEvent(new CommentRejectedEvent(Id, reasons, moderatorId));
    }

    /// <summary>
    /// Increment reaction count
    /// </summary>
    public void IncrementReactionCount(int count = 1)
    {
        ReactionCount = Math.Max(0, ReactionCount + count);
        AddDomainEvent(new CommentReactionCountChangedEvent(Id, count));
    }

    /// <summary>
    /// Decrement reaction count
    /// </summary>
    public void DecrementReactionCount(int count = 1)
    {
        ReactionCount = Math.Max(0, ReactionCount - count);
        AddDomainEvent(new CommentReactionCountChangedEvent(Id, -count));
    }

    /// <summary>
    /// Increment reply count
    /// </summary>
    public void IncrementReplyCount(int count = 1)
    {
        ReplyCount = Math.Max(0, ReplyCount + count);
        AddDomainEvent(new CommentReplyCountChangedEvent(Id, count));
    }

    /// <summary>
    /// Decrement reply count
    /// </summary>
    public void DecrementReplyCount(int count = 1)
    {
        ReplyCount = Math.Max(0, ReplyCount - count);
        AddDomainEvent(new CommentReplyCountChangedEvent(Id, -count));
    }

    /// <summary>
    /// Soft delete comment
    /// </summary>
    public void SoftDelete(Guid deleterAliasId)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByAliasId = deleterAliasId.ToString();

        AddDomainEvent(new CommentDeletedEvent(Id, PostId, deleterAliasId));
    }

    /// <summary>
    /// Restore deleted comment
    /// </summary>
    public void Restore(Guid restorerAliasId)
    {
        if (!IsDeleted) return;

        if (restorerAliasId != Author.AliasId)
            throw new CommentAuthorMismatchException("Chỉ tác giả mới có thể khôi phục bình luận.");

        IsDeleted = false;
        DeletedAt = null;
        DeletedByAliasId = null;

        AddDomainEvent(new CommentRestoredEvent(Id, restorerAliasId));
    }

    // Business logic properties
    public bool IsRootComment => Hierarchy.IsRootComment;
    public bool IsReply => Hierarchy.IsReply;
    public bool IsEdited => EditedAt != default;
    public bool HasReplies => ReplyCount > 0;
    public bool HasReactions => ReactionCount > 0;
    public bool IsPopular => ReactionCount > 10 || ReplyCount > 5;
    public bool IsDeepNested => Hierarchy.IsDeepNested;
    public bool CanReply => !IsDeleted && Hierarchy.Level < 4; // Max 5 levels

    // Private validation methods
    private void ValidateEditPermission(Guid editorAliasId)
    {
        if (editorAliasId != Author.AliasId)
            throw new CommentAuthorMismatchException();
    }

    private void ValidateNotDeleted()
    {
        if (IsDeleted)
            throw new DeletedCommentActionException("Không thể thực hiện hành động trên bình luận đã bị xóa.");
    }
}
