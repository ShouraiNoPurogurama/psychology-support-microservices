using Conversation.Domain.Abstractions;
using Conversation.Domain.Aggregates.Conversations.ValueObjects;
using Conversation.Domain.Aggregates.Conversations.Enums;
using Conversation.Domain.Aggregates.Conversations.DomainEvents;
using Conversation.Domain.Exceptions;

namespace Conversation.Domain.Aggregates.Conversations;

public sealed class Conversation : AggregateRoot<string>
{
    // Value Objects
    public LastMessageSummary? LastMessage { get; private set; }
    public SummarizationInfo? AiSummary { get; private set; }

    // Properties
    public ConversationType Type { get; private set; }
    public string? Title { get; private set; }
    public bool IsArchived { get; private set; }
    public long NextSeq { get; private set; }
    public int TotalMessages { get; private set; }
    public DateTimeOffset? LastActivityAt { get; private set; }

    // Collections - Private backing fields with public readonly access
    private readonly List<Message> _messages = new();
    private readonly List<Participant> _participants = new();
    private readonly List<TypingStatusInfo> _typingStatuses = new();

    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();
    public IReadOnlyList<TypingStatusInfo> TypingStatuses => _typingStatuses.AsReadOnly();

    // EF Core constructor
    private Conversation() { }

    private Conversation(
        string id,
        ConversationType type,
        string? title,
        List<ParticipantInfo> initialParticipants)
    {
        Id = id;
        Type = type;
        Title = title;
        IsArchived = false;
        NextSeq = 1;
        TotalMessages = 0;
        LastActivityAt = DateTimeOffset.UtcNow;

        // Add initial participants
        foreach (var participantInfo in initialParticipants)
        {
            var participant = Participant.Create(Id, participantInfo);
            _participants.Add(participant);
        }
    }

    public static Conversation Create(
        string conversationId,
        ConversationType type,
        List<ParticipantInfo> initialParticipants,
        string? title = null)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new InvalidConversationStateException("Conversation ID cannot be empty.");

        if (initialParticipants == null || initialParticipants.Count == 0)
            throw new InvalidConversationStateException("Conversation must have at least one participant.");

        if (type == ConversationType.OneOnOne && initialParticipants.Count != 2)
            throw new InvalidConversationStateException("One-on-one conversation must have exactly 2 participants.");

        if (type == ConversationType.Group && string.IsNullOrWhiteSpace(title))
            throw new InvalidConversationStateException("Group conversation must have a title.");

        var conversation = new Conversation(conversationId, type, title, initialParticipants);

        // Raise domain event
        conversation.AddDomainEvent(new ConversationCreatedEvent(
            conversationId,
            type,
            initialParticipants.Select(p => p.AliasId).ToList(),
            DateTimeOffset.UtcNow));

        return conversation;
    }

    public void SendMessage(Guid senderAliasId, MessageContent content)
    {
        ValidateNotArchived();
        ValidateParticipantExists(senderAliasId);

        var senderParticipant = _participants.First(p => p.Info.AliasId == senderAliasId);
        if (!senderParticipant.IsActive)
            throw new ConversationAccessException("Inactive participants cannot send messages.");

        var message = Message.Create(
            Id,
            NextSeq,
            senderAliasId,
            senderParticipant.Info.DisplayName,
            content);

        _messages.Add(message);
        NextSeq++;
        TotalMessages++;
        LastActivityAt = DateTimeOffset.UtcNow;

        // Update last message summary
        LastMessage = LastMessageSummary.Create(
            content.Text,
            senderAliasId,
            senderParticipant.Info.DisplayName,
            message.SentAt,
            message.Seq);

        // Clear typing status for sender
        _typingStatuses.RemoveAll(t => t.ParticipantAliasId == senderAliasId);

        // Raise domain event
        AddDomainEvent(new MessageSentEvent(
            Id,
            message.Id,
            message.Seq,
            senderAliasId,
            content.Text,
            message.SentAt));
    }

    public void AddParticipant(ParticipantInfo participantInfo, Guid addedByAliasId)
    {
        ValidateNotArchived();
        ValidateParticipantExists(addedByAliasId);

        if (_participants.Any(p => p.Info.AliasId == participantInfo.AliasId && p.IsActive))
            throw new InvalidParticipantException("Participant is already in the conversation.");

        if (Type == ConversationType.OneOnOne)
            throw new InvalidConversationStateException("Cannot add participants to one-on-one conversation.");

        var participant = Participant.Create(Id, participantInfo);
        _participants.Add(participant);
        LastActivityAt = DateTimeOffset.UtcNow;

        // Raise domain event
        AddDomainEvent(new ParticipantAddedEvent(
            Id,
            participantInfo.AliasId,
            participantInfo.DisplayName,
            addedByAliasId,
            DateTimeOffset.UtcNow));
    }

    public void RemoveParticipant(Guid participantAliasId, Guid removedByAliasId)
    {
        ValidateNotArchived();
        ValidateParticipantExists(removedByAliasId);
        ValidateParticipantExists(participantAliasId);

        if (Type == ConversationType.OneOnOne)
            throw new InvalidConversationStateException("Cannot remove participants from one-on-one conversation.");

        var participant = _participants.First(p => p.Info.AliasId == participantAliasId);
        participant.Leave();
        LastActivityAt = DateTimeOffset.UtcNow;

        // Clear typing status
        _typingStatuses.RemoveAll(t => t.ParticipantAliasId == participantAliasId);

        // Raise domain event
        AddDomainEvent(new ParticipantRemovedEvent(
            Id,
            participantAliasId,
            removedByAliasId,
            DateTimeOffset.UtcNow));
    }

    public void MarkAsRead(Guid participantAliasId, long lastReadSeq)
    {
        ValidateParticipantExists(participantAliasId);

        var participant = _participants.First(p => p.Info.AliasId == participantAliasId);
        if (!participant.IsActive)
            throw new ConversationAccessException("Inactive participants cannot mark messages as read.");

        if (lastReadSeq <= participant.Info.LastReadSeq)
            return; // Already read or invalid sequence

        participant.UpdateLastReadSeq(lastReadSeq);

        // Mark messages as read
        var messagesToMarkAsRead = _messages
            .Where(m => m.Seq <= lastReadSeq && m.SenderAliasId != participantAliasId)
            .ToList();

        foreach (var message in messagesToMarkAsRead)
        {
            message.MarkAsRead();
        }

        // Raise domain event
        AddDomainEvent(new MessageReadEvent(
            Id,
            participantAliasId,
            lastReadSeq,
            DateTimeOffset.UtcNow));
    }

    public void ApplySummarization(SummarizationInfo summaryInfo)
    {
        AiSummary = summaryInfo;

        // Raise domain event
        AddDomainEvent(new ConversationSummarizedEvent(
            Id,
            summaryInfo.Summary,
            summaryInfo.Model,
            summaryInfo.MessagesAnalyzed,
            summaryInfo.GeneratedAt));
    }

    public void UpdateTypingStatus(Guid participantAliasId, bool isTyping)
    {
        ValidateParticipantExists(participantAliasId);

        var participant = _participants.First(p => p.Info.AliasId == participantAliasId);
        if (!participant.IsActive)
            return; // Don't update typing status for inactive participants

        // Remove existing typing status for this participant
        _typingStatuses.RemoveAll(t => t.ParticipantAliasId == participantAliasId);

        // Add new typing status if typing
        if (isTyping)
        {
            var typingStatus = TypingStatusInfo.Create(participantAliasId, isTyping);
            _typingStatuses.Add(typingStatus);
        }

        // Raise domain event
        AddDomainEvent(new TypingStatusChangedEvent(
            Id,
            participantAliasId,
            isTyping,
            DateTimeOffset.UtcNow));
    }

    public void EditMessage(Guid messageId, MessageContent newContent, Guid editorAliasId)
    {
        ValidateNotArchived();
        ValidateParticipantExists(editorAliasId);

        var message = _messages.FirstOrDefault(m => m.Id == messageId);
        if (message == null)
            throw new InvalidConversationStateException("Message not found.");

        if (message.SenderAliasId != editorAliasId)
            throw new ConversationAccessException("Only the message sender can edit the message.");

        if (message.IsDeleted)
            throw new InvalidConversationStateException("Cannot edit deleted message.");

        var oldContent = message.Content.Text;
        message.Edit(newContent);
        LastActivityAt = DateTimeOffset.UtcNow;

        // Update last message summary if this was the last message
        if (LastMessage?.Seq == message.Seq)
        {
            var senderParticipant = _participants.First(p => p.Info.AliasId == message.SenderAliasId);
            LastMessage = LastMessageSummary.Create(
                newContent.Text,
                message.SenderAliasId,
                senderParticipant.Info.DisplayName,
                message.SentAt,
                message.Seq);
        }

        // Raise domain event
        AddDomainEvent(new MessageEditedEvent(
            Id,
            messageId,
            oldContent,
            newContent.Text,
            editorAliasId,
            message.EditedAt!.Value));
    }

    public void DeleteMessage(Guid messageId, Guid deletedByAliasId)
    {
        ValidateNotArchived();
        ValidateParticipantExists(deletedByAliasId);

        var message = _messages.FirstOrDefault(m => m.Id == messageId);
        if (message == null)
            throw new InvalidConversationStateException("Message not found.");

        if (message.SenderAliasId != deletedByAliasId)
            throw new ConversationAccessException("Only the message sender can delete the message.");

        if (message.IsDeleted)
            return; // Already deleted

        message.Delete();
        LastActivityAt = DateTimeOffset.UtcNow;

        // Update last message summary if this was the last message
        if (LastMessage?.Seq == message.Seq)
        {
            var previousMessage = _messages
                .Where(m => !m.IsDeleted && m.Seq < message.Seq)
                .OrderByDescending(m => m.Seq)
                .FirstOrDefault();

            if (previousMessage != null)
            {
                var senderParticipant = _participants.First(p => p.Info.AliasId == previousMessage.SenderAliasId);
                LastMessage = LastMessageSummary.Create(
                    previousMessage.Content.Text,
                    previousMessage.SenderAliasId,
                    senderParticipant.Info.DisplayName,
                    previousMessage.SentAt,
                    previousMessage.Seq);
            }
            else
            {
                LastMessage = null;
            }
        }

        // Raise domain event
        AddDomainEvent(new MessageDeletedEvent(
            Id,
            messageId,
            deletedByAliasId,
            DateTimeOffset.UtcNow));
    }

    public void Archive(Guid archivedByAliasId)
    {
        ValidateParticipantExists(archivedByAliasId);

        if (IsArchived)
            return; // Already archived

        IsArchived = true;
        LastActivityAt = DateTimeOffset.UtcNow;

        // Clear all typing statuses
        _typingStatuses.Clear();

        // Raise domain event
        AddDomainEvent(new ConversationArchivedEvent(
            Id,
            archivedByAliasId,
            DateTimeOffset.UtcNow));
    }

    public void Unarchive(Guid unarchivedByAliasId)
    {
        ValidateParticipantExists(unarchivedByAliasId);

        if (!IsArchived)
            return; // Not archived

        IsArchived = false;
        LastActivityAt = DateTimeOffset.UtcNow;
    }

    // Helper methods for validation
    private void ValidateNotArchived()
    {
        if (IsArchived)
            throw new InvalidConversationStateException("Operation not allowed on archived conversation.");
    }

    private void ValidateParticipantExists(Guid participantAliasId)
    {
        if (!_participants.Any(p => p.Info.AliasId == participantAliasId && p.IsActive))
            throw new ConversationAccessException("Participant is not in the conversation or is inactive.");
    }

    // Public properties for business logic
    public bool HasActiveParticipants => _participants.Any(p => p.IsActive);
    public int ActiveParticipantCount => _participants.Count(p => p.IsActive);
    public bool HasUnreadMessages(Guid participantAliasId)
    {
        var participant = _participants.FirstOrDefault(p => p.Info.AliasId == participantAliasId);
        return participant != null && TotalMessages > 0 && participant.Info.LastReadSeq < NextSeq - 1;
    }
}
