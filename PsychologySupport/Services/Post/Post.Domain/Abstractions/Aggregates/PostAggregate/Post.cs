using Post.Domain.Enums;
using Post.Domain.Events;
using Post.Domain.Exceptions;
using Post.Domain.Abstractions.Aggregates.PostAggregate.ValueObjects;

namespace Post.Domain.Abstractions.Aggregates.PostAggregate;

public sealed class Post : AggregateRoot<Guid>, ISoftDeletable
{
    // Value Objects
    public PostContent Content { get; private set; } = null!;
    public AuthorInfo Author { get; private set; } = null!;
    public ModerationInfo Moderation { get; private set; } = null!;
    public PostMetrics Metrics { get; private set; } = null!;

    // Properties
    public PostVisibility Visibility { get; private set; } = PostVisibility.Draft;
    public DateTime PublishedAt { get; private set; }
    public DateTime? EditedAt { get; private set; }

    // Collections
    private readonly List<PostMedia> _media = new();
    private readonly List<PostCategory> _categories = new();
    private readonly List<PostEmotion> _emotions = new();

    public IReadOnlyList<PostMedia> Media => _media.AsReadOnly();
    public IReadOnlyList<PostCategory> Categories => _categories.AsReadOnly();
    public IReadOnlyList<PostEmotion> Emotions => _emotions.AsReadOnly();

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

    private Post()
    {
    }

    /// <summary>
    /// Factory method to create a new post with proper validation
    /// </summary>
    public static Post Create(
        Guid authorAliasId,
        string content,
        string? title = null,
        Guid? authorAliasVersionId = null,
        PostVisibility visibility = PostVisibility.Draft)
    {
        var postContent = new PostContent(content, title);
        var author = new AuthorInfo(authorAliasId, authorAliasVersionId);
        var moderation = new ModerationInfo();
        var metrics = new PostMetrics();

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Content = postContent,
            Author = author,
            Moderation = moderation,
            Metrics = metrics,
            Visibility = visibility,
            PublishedAt = DateTime.UtcNow
        };

        post.AddDomainEvent(new PostCreatedEvent(post.Id, post.Author.AliasId));
        return post;
    }

    /// <summary>
    /// Update post content with validation
    /// </summary>
    public void UpdateContent(string newContent, string? newTitle, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        var oldContent = Content;
        Content = new PostContent(newContent, newTitle);
        EditedAt = DateTime.UtcNow;

        AddDomainEvent(new PostContentUpdatedEvent(Id, oldContent.Value, Content.Value));
    }

    /// <summary>
    /// Change post visibility with business rules
    /// </summary>
    public void ChangeVisibility(PostVisibility newVisibility, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        if (Visibility == newVisibility) return;

        // Business rule: Can only publish if approved
        if (newVisibility == PostVisibility.Public && !Moderation.IsApproved)
            throw new InvalidPostDataException("Chỉ có thể công khai bài viết đã được duyệt.");

        var oldVisibility = Visibility;
        Visibility = newVisibility;

        AddDomainEvent(new PostVisibilityChangedEvent(Id, oldVisibility, newVisibility));
    }

    /// <summary>
    /// Approve post for publication
    /// </summary>
    public void Approve(string policyVersion, Guid moderatorId)
    {
        Moderation = Moderation.Approve(policyVersion);
        AddDomainEvent(new PostApprovedEvent(Id, moderatorId));
    }

    /// <summary>
    /// Reject post with reasons
    /// </summary>
    public void Reject(List<string> reasons, string policyVersion, Guid moderatorId)
    {
        Moderation = Moderation.Reject(reasons, policyVersion);
        AddDomainEvent(new PostRejectedEvent(Id, reasons, moderatorId));
    }

    /// <summary>
    /// Add media to post with validation
    /// </summary>
    public void AddMedia(Guid mediaId, int? position = null)
    {
        ValidateNotDeleted();

        if (_media.Any(m => m.MediaId == mediaId))
            throw new InvalidPostDataException("Media đã được thêm vào bài viết.");

        if (_media.Count >= 10) // Business rule: max 10 media per post
            throw new InvalidPostDataException("Bài viết chỉ có thể chứa tối đa 10 media.");

        var postMedia = PostMedia.Create(Id, mediaId, position);

        _media.Add(postMedia);
        AddDomainEvent(new PostMediaAddedEvent(Id, mediaId));
    }

    /// <summary>
    /// Remove media from post
    /// </summary>
    public void RemoveMedia(Guid mediaId, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        var media = _media.FirstOrDefault(m => m.MediaId == mediaId);
        if (media == null) return;

        _media.Remove(media);
        AddDomainEvent(new PostMediaRemovedEvent(Id, mediaId));
    }

    /// <summary>
    /// Add category to post
    /// </summary>
    public void AddCategory(Guid categoryTagId)
    {
        ValidateNotDeleted();

        if (_categories.Any(c => c.CategoryTagId == categoryTagId))
            return; // Already exists

        if (_categories.Count >= 5) // Business rule: max 5 categories per post
            throw new InvalidPostDataException("Bài viết chỉ có thể có tối đa 5 danh mục.");

        var postCategory = PostCategory.Create(Id, categoryTagId);

        _categories.Add(postCategory);
        AddDomainEvent(new PostCategoryAddedEvent(Id, categoryTagId));
    }

    /// <summary>
    /// Remove category from post
    /// </summary>
    public void RemoveCategory(Guid categoryTagId, Guid editorAliasId)
    {
        ValidateEditPermission(editorAliasId);
        ValidateNotDeleted();

        var category = _categories.FirstOrDefault(c => c.CategoryTagId == categoryTagId);
        if (category == null) return;

        _categories.Remove(category);
        AddDomainEvent(new PostCategoryRemovedEvent(Id, categoryTagId));
    }

    /// <summary>
    /// Increment reaction count
    /// </summary>
    public void IncrementReactionCount(int count = 1)
    {
        Metrics = Metrics.IncrementReactions(count);
        AddDomainEvent(new PostMetricsUpdatedEvent(Id, "reaction", count));
    }

    /// <summary>
    /// Decrement reaction count
    /// </summary>
    public void DecrementReactionCount(int count = 1)
    {
        Metrics = Metrics.DecrementReactions(count);
        AddDomainEvent(new PostMetricsUpdatedEvent(Id, "reaction", -count));
    }

    /// <summary>
    /// Increment comment count
    /// </summary>
    public void IncrementCommentCount(int count = 1)
    {
        Metrics = Metrics.IncrementComments(count);
        AddDomainEvent(new PostMetricsUpdatedEvent(Id, "comment", count));
    }

    /// <summary>
    /// Decrement comment count
    /// </summary>
    public void DecrementCommentCount(int count = 1)
    {
        Metrics = Metrics.DecrementComments(count);
        AddDomainEvent(new PostMetricsUpdatedEvent(Id, "comment", -count));
    }

    /// <summary>
    /// Record post view
    /// </summary>
    public void RecordView()
    {
        if (IsDeleted || Visibility == PostVisibility.Draft) return;

        Metrics = Metrics.IncrementViews();
        AddDomainEvent(new PostViewedEvent(Id));
    }

    /// <summary>
    /// Soft delete post
    /// </summary>
    public void SoftDelete(Guid deleterAliasId)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByAliasId = deleterAliasId.ToString();

        AddDomainEvent(new PostDeletedEvent(Id));
    }

    /// <summary>
    /// Restore deleted post
    /// </summary>
    public void Restore(Guid restorerAliasId)
    {
        if (!IsDeleted) return;

        // Business rule: Only author or admin can restore
        if (restorerAliasId != Author.AliasId)
            throw new PostAuthorMismatchException("Chỉ tác giả mới có thể khôi phục bài viết.");

        IsDeleted = false;
        DeletedAt = null;
        DeletedByAliasId = null;

        AddDomainEvent(new PostRestoredEvent(Id, restorerAliasId));
    }

    // Business logic properties
    public bool CanBePublished => Moderation.IsApproved && !IsDeleted;
    public bool IsPublished => Visibility == PostVisibility.Public && CanBePublished;
    public bool IsEdited => EditedAt.HasValue;
    public bool IsPopular => Metrics.IsPopular;
    public bool IsTrending => Metrics.IsTrending;
    public bool HasMedia => _media.Any();
    public bool HasCategories => _categories.Any();

    // Private validation methods
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