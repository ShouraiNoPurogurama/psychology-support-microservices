using Post.Domain.Enums;
using Post.Domain.Events;
using Post.Domain.Exceptions;

namespace Post.Domain.Posts;

public partial class Post : AggregateRoot<Guid>, ISoftDeletable
{
    public PostVisibility Visibility { get; private set; } = PostVisibility.Draft;

    public string? Title { get; private set; }

    public string Content { get; private set; } = null!;

    public Guid AuthorAliasId { get; private set; }

    public Guid AuthorAliasVersionId { get; private set; }

    public ModerationStatus ModerationStatus { get; private set; } = ModerationStatus.Pending;

    public List<string> ModerationReasons { get; private set; } = null!;

    public string? ModerationPolicyVersion { get; private set; }

    public int ReactionCount { get; private set; }

    public int CommentCount { get; private set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public string? DeletedByAliasId { get; set; }

    private Post()
    {
    }

    /// <summary>
    /// Factory Method để tạo một bài viết mới, đảm bảo nó luôn ở trạng thái hợp lệ ban đầu.
    /// </summary>
    public static Post Create(Guid authorAliasId,
        Guid authorAliasVersionId,
        string content,
        string? title = null,
        PostVisibility visibility = PostVisibility.Public
    )
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidPostDataException("Nội dung bài viết không được để trống.");
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            AuthorAliasId = authorAliasId,
            AuthorAliasVersionId = authorAliasVersionId,
            Content = content,
            Title = title,
            Visibility = visibility,
            ModerationStatus = ModerationStatus.Pending, 
            ReactionCount = 0,
            CommentCount = 0
        };
        
        post.AddDomainEvent(new PostCreatedEvent(post.Id, post.AuthorAliasId));

        return post;
    }

    public void UpdateContent(string newContent, string? newTitle, Guid editorAliasId)
    {
        if (editorAliasId != AuthorAliasId)
            throw new PostAuthorMismatchException();
        
        if(IsDeleted) 
            throw new DeletedPostActionException("Không thể cập nhật nội dung của một bài viết đã bị xóa.");
        
        if (string.IsNullOrWhiteSpace(newContent))
        {
            throw new InvalidPostDataException("Nội dung bài viết không được để trống.");
        }

        Content = newContent;
        Title = newTitle;
    }
    
    /// <summary>
    /// Xóa mềm bài viết.
    /// </summary>
    public void SoftDelete(Guid deleterAliasId)
    {
        if (IsDeleted)
        {
            return; 
        }

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByAliasId = deleterAliasId.ToString();
        
        AddDomainEvent(new PostDeletedEvent(Id));
    }

    /// <summary>
    /// Duyệt và cho phép hiển thị bài viết.
    /// </summary>
    public void Approve(string policyVersion)
    {
        if (ModerationStatus != ModerationStatus.Pending)
        {
            throw new InvalidPostModerationStateException("Chỉ các bài viết đang chờ duyệt mới có thể được phê duyệt.");
        }

        ModerationStatus = ModerationStatus.Approved;
        ModerationPolicyVersion = policyVersion;
        ModerationReasons.Clear();
        
        AddDomainEvent(new PostApprovedEvent(Id));
    }

    /// <summary>
    /// Từ chối bài viết.
    /// </summary>
    public void Reject(List<string> reasons, string policyVersion)
    {
        if (ModerationStatus != ModerationStatus.Pending)
        {
            throw new InvalidPostModerationStateException("Chỉ các bài viết đang chờ duyệt mới có thể được phê duyệt.");
        }
        
        if (reasons == null || reasons.Count == 0)
        {
            throw new InvalidPostModerationStateException("Phải cung cấp ít nhất một lý do từ chối duyệt.");
        }

        ModerationStatus = ModerationStatus.Rejected;
        ModerationReasons = reasons;
        ModerationPolicyVersion = policyVersion;

        AddDomainEvent(new PostRejectedEvent(Id, reasons));
    }
}