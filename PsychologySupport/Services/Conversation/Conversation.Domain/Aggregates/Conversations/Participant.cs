using Conversation.Domain.Abstractions;
using Conversation.Domain.Aggregates.Conversations.Enums;
using Conversation.Domain.Aggregates.Conversations.ValueObjects;

namespace Conversation.Domain.Aggregates.Conversations;

public sealed class Participant : Entity<Guid>
{
    public string ConversationId { get; private set; } = default!;
    public ParticipantInfo Info { get; private set; } = default!;
    public ParticipantRole Role { get; private set; }
    public bool IsMuted { get; private set; }
    public DateTimeOffset? LeftAt { get; private set; }

    // EF Core constructor
    private Participant() { }

    private Participant(
        string conversationId,
        ParticipantInfo info,
        ParticipantRole role)
    {
        Id = Guid.NewGuid();
        ConversationId = conversationId;
        Info = info;
        Role = role;
        IsMuted = false;
        LeftAt = null;
    }

    public static Participant Create(
        string conversationId,
        ParticipantInfo info,
        ParticipantRole role = ParticipantRole.Member)
    {
        return new Participant(conversationId, info, role);
    }

    public void UpdateLastReadSeq(long seq)
    {
        Info = Info.UpdateLastReadSeq(seq);
    }

    public void Leave()
    {
        LeftAt = DateTimeOffset.UtcNow;
        Info = Info.Deactivate();
    }

    public void Rejoin()
    {
        LeftAt = null;
        Info = Info with { IsActive = true };
    }

    public void Mute()
    {
        IsMuted = true;
    }

    public void Unmute()
    {
        IsMuted = false;
    }

    public void ChangeRole(ParticipantRole newRole)
    {
        Role = newRole;
    }

    public bool IsActive => Info.IsActive && LeftAt == null;
}