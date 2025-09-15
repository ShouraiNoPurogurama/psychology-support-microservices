using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.Post.ValueObjects;

public sealed record PostContent
{
    public string Value { get; init; } = default!;
    public string? Title { get; init; }
    public int WordCount { get; init; }
    public int CharacterCount { get; init; }

    // EF Core materialization
    private PostContent() { }

    // Ctor kín – chỉ factory gọi
    private PostContent(string value, string? title, int wordCount, int charCount)
    {
        Value = value;
        Title = title;
        WordCount = wordCount;
        CharacterCount = charCount;
    }

    public static PostContent Create(string content, string? title = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidPostDataException("Nội dung bài viết không được để trống.");

        if (content.Length > 10000)
            throw new InvalidPostDataException("Nội dung bài viết không được vượt quá 10,000 ký tự.");

        if (!string.IsNullOrEmpty(title) && title.Length > 200)
            throw new InvalidPostDataException("Tiêu đề bài viết không được vượt quá 200 ký tự.");

        var v = content.Trim();
        var t = title?.Trim();

        return new PostContent(
            value: v,
            title: t,
            wordCount: CountWords(v),
            charCount: v.Length
        );
    }

    private static int CountWords(string text)
        => string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

    public bool HasTitle  => !string.IsNullOrWhiteSpace(Title);
    public bool IsLongForm  => WordCount > 100;
    public bool IsShortForm => WordCount <= 100;
}