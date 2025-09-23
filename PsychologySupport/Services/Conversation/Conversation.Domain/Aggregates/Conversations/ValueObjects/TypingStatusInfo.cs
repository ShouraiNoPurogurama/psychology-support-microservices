using Conversation.Domain.Exceptions;

namespace Conversation.Domain.Aggregates.Conversations.ValueObjects;

public sealed record TypingStatusInfo
{
    public Guid ParticipantAliasId { get; init; }
    public bool IsTyping { get; init; }
    public DateTimeOffset LastUpdated { get; init; }

    private TypingStatusInfo() { }

    private TypingStatusInfo(Guid participantAliasId, bool isTyping, DateTimeOffset lastUpdated)
    {
        ParticipantAliasId = participantAliasId;
        IsTyping = isTyping;
        LastUpdated = lastUpdated;
    }

    public static TypingStatusInfo Create(Guid participantAliasId, bool isTyping)
    {
        if (participantAliasId == Guid.Empty)
            throw new InvalidParticipantException("Participant alias ID cannot be empty.");

        return new TypingStatusInfo(
            participantAliasId: participantAliasId,
            isTyping: isTyping,
            lastUpdated: DateTimeOffset.UtcNow
        );
    }
}