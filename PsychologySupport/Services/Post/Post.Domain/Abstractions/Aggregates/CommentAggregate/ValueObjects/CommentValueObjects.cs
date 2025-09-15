using Post.Domain.Enums;
using Post.Domain.Events;
using Post.Domain.Exceptions;

namespace Post.Domain.Abstractions.Aggregates.CommentAggregate.ValueObjects;

public sealed record CommentContent
{
    public string Value { get; init; }
    public int CharacterCount { get; init; }
    public int WordCount { get; init; }

    public CommentContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidCommentDataException("Nội dung bình luận không được để trống.");

        if (content.Length > 2000) // Max comment length
            throw new InvalidCommentDataException("Bình luận không được vượt quá 2,000 ký tự.");

        Value = content.Trim();
        CharacterCount = Value.Length;
        WordCount = CountWords(Value);
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public bool IsLongComment => WordCount > 50;
    public bool IsShortComment => WordCount <= 10;
}

public sealed record CommentHierarchy
{
    public string Path { get; init; }
    public int Level { get; init; }
    public Guid? ParentCommentId { get; init; }

    public CommentHierarchy(Guid? parentCommentId = null, string? parentPath = null)
    {
        ParentCommentId = parentCommentId;
        
        if (parentCommentId.HasValue)
        {
            if (string.IsNullOrEmpty(parentPath))
                throw new InvalidCommentDataException("Parent path is required when parent comment ID is provided.");
            
            Level = parentPath.Split('/').Length;
            
            if (Level >= 5) // Max nesting level
                throw new InvalidCommentDataException("Bình luận chỉ có thể lồng tối đa 5 cấp.");
            
            Path = $"{parentPath}/{parentCommentId}";
        }
        else
        {
            Level = 0;
            Path = "";
        }
    }

    public bool IsRootComment => !ParentCommentId.HasValue;
    public bool IsReply => ParentCommentId.HasValue;
    public bool IsDeepNested => Level >= 3;
}
