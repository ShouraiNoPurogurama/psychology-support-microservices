using Post.Domain.Exceptions;

namespace Post.Domain.Abstractions.Aggregates.PostAggregate.ValueObjects;

public sealed record PostContent
{
    public string Value { get; init; }
    public string? Title { get; init; }
    public int WordCount { get; init; }
    public int CharacterCount { get; init; }

    public PostContent(string content, string? title = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidPostDataException("Nội dung bài viết không được để trống.");

        if (content.Length > 10000) // Max content length
            throw new InvalidPostDataException("Nội dung bài viết không được vượt quá 10,000 ký tự.");

        if (!string.IsNullOrEmpty(title) && title.Length > 200)
            throw new InvalidPostDataException("Tiêu đề bài viết không được vượt quá 200 ký tự.");

        Value = content.Trim();
        Title = title?.Trim();
        WordCount = CountWords(Value);
        CharacterCount = Value.Length;
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);
    public bool IsLongForm => WordCount > 100;
    public bool IsShortForm => WordCount <= 100;
}
