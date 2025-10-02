namespace Post.Domain.Aggregates.Comments.ValueObjects;

public sealed record CommentHierarchy
{
    private const int MaxNestingLevel = 5;

    public string Path { get; init; } = string.Empty;
    public int Level { get; init; }
    public Guid? ParentCommentId { get; init; }

    // Cho EF Core materialize
    private CommentHierarchy()
    {
    }

    // Ctor kín để đảm bảo tạo qua factory
    private CommentHierarchy(Guid? parentCommentId, string path, int level)
    {
        ParentCommentId = parentCommentId;
        Path = path;
        Level = level;
    }

    /// <summary>
    /// Factory method để tạo hierarchy cho một comment.
    /// Nó tự xử lý tất cả logic về path và level.
    /// </summary>
    /// <param name="parentComment">Comment cha (nếu là reply) hoặc null (nếu là comment gốc).</param>
    public static CommentHierarchy Create(Comment? parentComment)
    {
        // 1. Nếu là comment gốc (không có cha)
        if (parentComment is null)
        {
            // Level 0, không có path, không có parentId
            return new CommentHierarchy(null, string.Empty, 0);
        }
        
        // 2. Nếu là comment trả lời (có cha)
        var parentHierarchy = parentComment.Hierarchy;
        var newLevel = parentHierarchy.Level + 1;

        // 3. Kiểm tra logic lồng cấp
        if (newLevel >= MaxNestingLevel)
        {
            // User check > 5 (trong handler) và >= 5 (trong domain)
            // Thống nhất: >= 5 nghĩa là 0, 1, 2, 3, 4 là 5 cấp. Cấp 5 (newLevel=5) là không hợp lệ.
            newLevel = MaxNestingLevel;
        }
        
        // 4. Tạo path mới
        // Path mới là path của cha, cộng thêm ID của cha
        var parentPath = parentHierarchy.Path.Trim().Trim('/');
        var newPath = string.IsNullOrEmpty(parentPath)
            ? parentComment.Id.ToString()
            : $"{parentPath}/{parentComment.Id}";

        return new CommentHierarchy(parentComment.Id, newPath, newLevel);
    }


    public bool IsRootComment => !ParentCommentId.HasValue;
    public bool IsReply => ParentCommentId.HasValue;
    public bool IsDeepNested => Level >= 3;
}