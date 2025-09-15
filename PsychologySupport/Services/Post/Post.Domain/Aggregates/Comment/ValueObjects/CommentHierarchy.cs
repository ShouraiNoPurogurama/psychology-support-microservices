using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.CommentAggregate.ValueObjects;

public sealed record CommentHierarchy
{
    private const int MaxNestingLevel = 5;

    public string Path { get; init; } = string.Empty;
    public int Level { get; init; }
    public Guid? ParentCommentId { get; init; }

    // Cho EF Core materialize
    private CommentHierarchy() { }

    // Ctor kín để đảm bảo tạo qua factory
    private CommentHierarchy(Guid? parentCommentId, string path, int level)
    {
        ParentCommentId = parentCommentId;
        Path = path;
        Level = level;
    }

    public static CommentHierarchy Create(Guid? parentCommentId = null, string? parentPath = null)
    {
        if (parentCommentId is null)
        {
            // Root comment
            return new CommentHierarchy(null, string.Empty, 0);
        }

        if (string.IsNullOrWhiteSpace(parentPath))
            throw new InvalidCommentDataException("Parent path is required when parent comment ID is provided.");

        var currentLevel = parentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
        if (currentLevel >= MaxNestingLevel)
            throw new InvalidCommentDataException("Bình luận chỉ có thể lồng tối đa 5 cấp.");

        var normalizedParentPath = parentPath.Trim().Trim('/');
        var newPath = string.IsNullOrEmpty(normalizedParentPath)
            ? parentCommentId.Value.ToString()
            : $"{normalizedParentPath}/{parentCommentId}";

        return new CommentHierarchy(parentCommentId, newPath, currentLevel);
    }

    public bool IsRootComment => !ParentCommentId.HasValue;
    public bool IsReply => ParentCommentId.HasValue;
    public bool IsDeepNested => Level >= 3;
}