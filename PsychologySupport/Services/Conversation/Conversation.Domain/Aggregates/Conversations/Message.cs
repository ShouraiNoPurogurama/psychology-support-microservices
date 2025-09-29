using Conversation.Domain.Abstractions;
using Conversation.Domain.Aggregates.Conversations.ValueObjects;
using Conversation.Domain.Aggregates.Conversations.Enums;

namespace Conversation.Domain.Aggregates.Conversations;

public sealed class Message : Entity<Guid>
{
    public string ConversationId { get; private set; } = default!;
    public long Seq { get; private set; }
    public Guid SenderAliasId { get; private set; }
    public string SenderDisplayName { get; private set; } = default!;
    public MessageContent Content { get; private set; } = default!;
    public MessageStatus Status { get; private set; }
    public DateTimeOffset SentAt { get; private set; }
    public DateTimeOffset? EditedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    // EF Core constructor
    private Message() { }

    private Message(
        string conversationId,
        long seq,
        Guid senderAliasId,
        string senderDisplayName,
        MessageContent content,
        DateTimeOffset sentAt)
    {
        Id = Guid.NewGuid();
        ConversationId = conversationId;
        Seq = seq;
        SenderAliasId = senderAliasId;
        SenderDisplayName = senderDisplayName;
        Content = content;
        Status = MessageStatus.Sent;
        SentAt = sentAt;
        IsDeleted = false;
    }

    public static Message Create(
        string conversationId,
        long seq,
        Guid senderAliasId,
        string senderDisplayName,
        MessageContent content)
    {
        return new Message(
            conversationId,
            seq,
            senderAliasId,
            senderDisplayName,
            content,
            DateTimeOffset.UtcNow);
    }

    public void MarkAsDelivered()
    {
        if (Status == MessageStatus.Sent)
            Status = MessageStatus.Delivered;
    }

    public void MarkAsRead()
    {
        if (Status == MessageStatus.Delivered || Status == MessageStatus.Sent)
            Status = MessageStatus.Read;
    }

    public void Edit(MessageContent newContent)
    {
        Content = newContent;
        EditedAt = DateTimeOffset.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}