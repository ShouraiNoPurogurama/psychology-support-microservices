using Conversation.Domain.Exceptions;

namespace Conversation.Domain.Aggregates.Conversations.ValueObjects;

public sealed record LastMessageSummary
{
    public string Content { get; init; } = default!;
    public Guid SenderAliasId { get; init; }
    public string SenderDisplayName { get; init; } = default!;
    public DateTimeOffset SentAt { get; init; }
    public long Seq { get; init; }

    private LastMessageSummary() { }

    private LastMessageSummary(string content, Guid senderAliasId, string senderDisplayName, DateTimeOffset sentAt, long seq)
    {
        Content = content;
        SenderAliasId = senderAliasId;
        SenderDisplayName = senderDisplayName;
        SentAt = sentAt;
        Seq = seq;
    }

    public static LastMessageSummary Create(string content, Guid senderAliasId, string senderDisplayName, DateTimeOffset sentAt, long seq)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidMessageContentException("Last message content cannot be empty.");

        return new LastMessageSummary(
            content: content.Length > 100 ? content[..100] + "..." : content,
            senderAliasId: senderAliasId,
            senderDisplayName: senderDisplayName.Trim(),
            sentAt: sentAt,
            seq: seq
        );
    }
}