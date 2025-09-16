using Post.Domain.Aggregates.Posts.DomainEvents;
using Post.Domain.Aggregates.Posts.Enums;
using Post.Domain.Aggregates.Posts.ValueObjects;
using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.Posts;

public sealed class Post : AggregateRoot<Guid>, ISoftDeletable
{
    // Value Objects
    public PostContent   Content    { get; private set; } = null!;
    public AuthorInfo    Author     { get; private set; } = null!;
    public ModerationInfo Moderation { get; private set; } = null!;
    public PostMetrics   Metrics    { get; private set; } = null!;

    // Properties
    public PostVisibility Visibility { get; private set; } = PostVisibility.Draft;
    public bool IsAbandonmentEventEmitted { get; private set; } = false;
    public DateTime PublishedAt { get; private set; }
    public DateTime? EditedAt   { get; private set; }

    // Collections
    private readonly List<PostMedia>    _media      = new();
    private readonly List<PostCategory> _categories = new();
    private readonly List<PostEmotion>  _emotions   = new();

    public IReadOnlyList<PostMedia>    Media      => _media.AsReadOnly();
    public IReadOnlyList<PostCategory> Categories => _categories.AsReadOnly();
    public IReadOnlyList<PostEmotion>  Emotions   => _emotions.AsReadOnly();

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

    // EF Core materialization
    private Post() { }

    public static Post Create(
        Guid authorAliasId,
        string content,
        string? title = null,
        Guid? authorAliasVersionId = null,
        PostVisibility visibility = PostVisibility.Draft)
    {
        var post = new Post
        {
            Id          = Guid.NewGuid(),
            Content     = PostContent.Create(content, title),
            Author      = AuthorInfo.Create(authorAliasId, authorAliasVersionId),
            Moderation  = ModerationInfo.Pending(),
            Metrics     = PostMetrics.Create(),
            Visibility  = visibility,
            PublishedAt = DateTime.UtcNow
        };

        post.AddDomainEvent(new PostCreatedEvent(
            post.Id,
            post.Author.AliasId,
            post.Content.Value,
            post.Content.Title,
            post.Visibility.ToString(),
            new List<string>(),
            post.PublishedAt));
        return post;
    }

    public void UpdateContent(string newContent, string? newTitle, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        var oldContent = Content;
        Content = PostContent.Create(newContent, newTitle);
        EditedAt = DateTime.UtcNow;

        AddDomainEvent(new PostContentUpdatedEvent(Id, oldContent.Value, Content.Value, editorAliasId));
    }

    public void ChangeVisibility(PostVisibility newVisibility, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        if (Visibility == newVisibility) return;

        if (newVisibility == PostVisibility.Public && !Moderation.IsApproved)
            throw new InvalidPostDataException("Chỉ có thể công khai bài viết đã được duyệt.");

        var oldVisibility = Visibility;
        Visibility = newVisibility;

        AddDomainEvent(new PostVisibilityChangedEvent(Id, oldVisibility, newVisibility));
    }

    public void Approve(string policyVersion, Guid moderatorId)
    {
        Moderation = Moderation.Approve(policyVersion);
        AddDomainEvent(new PostApprovedEvent(Id, moderatorId));
    }

    public void Reject(List<string> reasons, string policyVersion, Guid moderatorId)
    {
        Moderation = Moderation.Reject(reasons, policyVersion);
        AddDomainEvent(new PostRejectedEvent(Id, reasons, moderatorId));
    }

    public void AddMedia(Guid mediaId, int? position = null)
    {
        ValidateNotDeleted();

        if (_media.Any(m => m.MediaId == mediaId))
            throw new InvalidPostDataException("Media đã được thêm vào bài viết.");

        if (_media.Count >= 10)
            throw new InvalidPostDataException("Bài viết chỉ có thể chứa tối đa 10 media.");

        var postMedia = PostMedia.Create(Id, mediaId, position);
        _media.Add(postMedia);

        AddDomainEvent(new PostMediaAddedEvent(Id, mediaId));
    }

    public void RemoveMedia(Guid mediaId, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        var media = _media.FirstOrDefault(m => m.MediaId == mediaId);
        if (media == null) return;

        _media.Remove(media);
        AddDomainEvent(new PostMediaRemovedEvent(Id, mediaId));
    }

    public void AddCategory(Guid categoryTagId)
    {
        ValidateNotDeleted();

        if (_categories.Any(c => c.CategoryTagId == categoryTagId))
            return;

        if (_categories.Count >= 5)
            throw new InvalidPostDataException("Bài viết chỉ có thể có tối đa 5 danh mục.");

        var postCategory = PostCategory.Create(Id, categoryTagId);
        _categories.Add(postCategory);

        AddDomainEvent(new PostCategoryAddedEvent(Id, categoryTagId));
    }

    public void RemoveCategory(Guid categoryTagId, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        var category = _categories.FirstOrDefault(c => c.CategoryTagId == categoryTagId);
        if (category == null) return;

        _categories.Remove(category);
        AddDomainEvent(new PostCategoryRemovedEvent(Id, categoryTagId));
    }

    public void IncrementReactionCount(int count = 1)
    {
        Metrics = Metrics.IncrementReactions(count);
        AddDomainEvent(new PostMetricsUpdatedEvent(Id, "reaction", count));
    }

    public void DecrementReactionCount(int count = 1)
    {
        Metrics = Metrics.DecrementReactions(count);
        AddDomainEvent(new PostMetricsUpdatedEvent(Id, "reaction", -count));
    }

    public void IncrementCommentCount(int count = 1)
    {
        Metrics = Metrics.IncrementComments(count);
        AddDomainEvent(new PostMetricsUpdatedEvent(Id, "comment", count));
    }

    public void DecrementCommentCount(int count = 1)
    {
        Metrics = Metrics.DecrementComments(count);
        AddDomainEvent(new PostMetricsUpdatedEvent(Id, "comment", -count));
    }

    public void RecordView()
    {
        if (IsDeleted || Visibility == PostVisibility.Draft) return;

        Metrics = Metrics.IncrementViews();
        AddDomainEvent(new PostViewedEvent(Id));
    }

    public void Delete(Guid deleterAliasId)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByAliasId = deleterAliasId.ToString();

        AddDomainEvent(new PostDeletedEvent(Id, deleterAliasId));
    }

    public void Restore(Guid restorerAliasId)
    {
        if (!IsDeleted) return;

        if (restorerAliasId != Author.AliasId)
            throw new PostAuthorMismatchException("Chỉ tác giả mới có thể khôi phục bài viết.");

        IsDeleted = false;
        DeletedAt = null;
        DeletedByAliasId = null;

        AddDomainEvent(new PostRestoredEvent(Id, restorerAliasId));
    }

    public void SynchronizeCounters(int reactionCount, int commentCount)
    {
        Metrics = Metrics with { ReactionCount = reactionCount, CommentCount = commentCount };
        AddDomainEvent(new PostMetricsUpdatedEvent(Id, "sync", 0));
    }

    public void MarkAbandonmentEventEmitted()
    {
        IsAbandonmentEventEmitted = true;
    }

    public void SoftDelete(Guid deleterAliasId)
    {
        ValidateEditPermission(deleterAliasId);
        Delete(deleterAliasId);
    }

    public bool CanBePublished => Moderation.IsApproved && !IsDeleted;
    public bool IsPublished    => Visibility == PostVisibility.Public && CanBePublished;
    public bool IsEdited       => EditedAt.HasValue;
    public bool IsPopular      => Metrics.IsPopular;
    public bool IsTrending     => Metrics.IsTrending;
    public bool HasMedia       => _media.Any();
    public bool HasCategories  => _categories.Any();

    private void ValidateEditPermission(Guid editorAliasId)
    {
        if (editorAliasId != Author.AliasId)
            throw new PostAuthorMismatchException();
    }

    private void ValidateNotDeleted()
    {
        if (IsDeleted)
            throw new DeletedPostActionException("Không thể thực hiện hành động trên bài viết đã bị xóa.");
    }
}
