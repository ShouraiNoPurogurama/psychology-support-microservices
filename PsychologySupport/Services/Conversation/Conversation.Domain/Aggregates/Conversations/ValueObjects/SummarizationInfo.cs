using Conversation.Domain.Exceptions;

namespace Conversation.Domain.Aggregates.Conversations.ValueObjects;

public sealed record SummarizationInfo
{
    public string Summary { get; init; } = default!;
    public string Model { get; init; } = default!;
    public DateTimeOffset GeneratedAt { get; init; }
    public int MessagesAnalyzed { get; init; }

    private SummarizationInfo() { }

    private SummarizationInfo(string summary, string model, DateTimeOffset generatedAt, int messagesAnalyzed)
    {
        Summary = summary;
        Model = model;
        GeneratedAt = generatedAt;
        MessagesAnalyzed = messagesAnalyzed;
    }

    public static SummarizationInfo Create(string summary, string model, int messagesAnalyzed)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new InvalidSummarizationException("Summary cannot be empty.");

        if (string.IsNullOrWhiteSpace(model))
            throw new InvalidSummarizationException("Model name cannot be empty.");

        if (messagesAnalyzed <= 0)
            throw new InvalidSummarizationException("Messages analyzed must be greater than zero.");

        return new SummarizationInfo(
            summary: summary.Trim(),
            model: model.Trim(),
            generatedAt: DateTimeOffset.UtcNow,
            messagesAnalyzed: messagesAnalyzed
        );
    }
}