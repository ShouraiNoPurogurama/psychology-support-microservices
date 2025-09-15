using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.Comment.ValueObjects;

public sealed record CommentContent
{
    public string Value { get; init; } = default!;
    public int CharacterCount { get; init; }
    public int WordCount { get; init; }

    // Cho EF Core materialize
    private CommentContent() { }

    // Ctor kín để đảm bảo tạo qua factory
    private CommentContent(string value, int charCount, int wordCount)
    {
        Value = value;
        CharacterCount = charCount;
        WordCount = wordCount;
    }

    public static CommentContent Create(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidCommentDataException("Nội dung bình luận không được để trống.");

        if (content.Length > 2000) // Max comment length
            throw new InvalidCommentDataException("Bình luận không được vượt quá 2,000 ký tự.");

        var trimmed = content.Trim();
        return new CommentContent(
            value: trimmed,
            charCount: trimmed.Length,
            wordCount: CountWords(trimmed)
        );
    }

    private static int CountWords(string text)
        => string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

    public bool IsLongComment => WordCount > 50;
    public bool IsShortComment => WordCount <= 10;
}