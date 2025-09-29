using Conversation.Domain.Abstractions;
using Conversation.Domain.Aggregates.Conversations.Enums;

namespace Conversation.Domain.Aggregates.Conversations.DomainEvents;

public sealed record ConversationCreatedEvent(
    string ConversationId,
    ConversationType Type,
    List<Guid> InitialParticipants,
    DateTimeOffset CreatedAt
) : DomainEvent(Guid.Parse(ConversationId));

public sealed record MessageSentEvent(
    string ConversationId,
    Guid MessageId,
    long Seq,
    Guid SenderAliasId,
    string Content,
    DateTimeOffset SentAt
) : DomainEvent(Guid.Parse(ConversationId));

public sealed record ParticipantAddedEvent(
    string ConversationId,
    Guid ParticipantAliasId,
    string DisplayName,
    Guid AddedByAliasId,
    DateTimeOffset AddedAt
) : DomainEvent(Guid.Parse(ConversationId));

public sealed record ParticipantRemovedEvent(
    string ConversationId,
    Guid ParticipantAliasId,
    Guid RemovedByAliasId,
    DateTimeOffset RemovedAt
) : DomainEvent(Guid.Parse(ConversationId));

public sealed record MessageReadEvent(
    string ConversationId,
    Guid ParticipantAliasId,
    long LastReadSeq,
    DateTimeOffset ReadAt
) : DomainEvent(Guid.Parse(ConversationId));

public sealed record ConversationSummarizedEvent(
    string ConversationId,
    string Summary,
    string Model,
    int MessagesAnalyzed,
    DateTimeOffset SummarizedAt
) : DomainEvent(Guid.Parse(ConversationId));

public sealed record TypingStatusChangedEvent(
    string ConversationId,
    Guid ParticipantAliasId,
    bool IsTyping,
    DateTimeOffset ChangedAt
) : DomainEvent(Guid.Parse(ConversationId));

public sealed record MessageEditedEvent(
    string ConversationId,
    Guid MessageId,
    string OldContent,
    string NewContent,
    Guid EditorAliasId,
    DateTimeOffset EditedAt
) : DomainEvent(Guid.Parse(ConversationId));

public sealed record MessageDeletedEvent(
    string ConversationId,
    Guid MessageId,
    Guid DeletedByAliasId,
    DateTimeOffset DeletedAt
) : DomainEvent(Guid.Parse(ConversationId));

public sealed record ConversationArchivedEvent(
    string ConversationId,
    Guid ArchivedByAliasId,
    DateTimeOffset ArchivedAt
) : DomainEvent(Guid.Parse(ConversationId));
