using Conversation.Domain.Exceptions;

namespace Conversation.Domain.Aggregates.Conversations.ValueObjects;

public sealed record ParticipantInfo
{
    public Guid AliasId { get; init; }
    public string DisplayName { get; init; } = default!;
    public DateTimeOffset JoinedAt { get; init; }
    public long LastReadSeq { get; init; }
    public bool IsActive { get; init; }

    private ParticipantInfo() { }

    private ParticipantInfo(Guid aliasId, string displayName, DateTimeOffset joinedAt, long lastReadSeq, bool isActive)
    {
        AliasId = aliasId;
        DisplayName = displayName;
        JoinedAt = joinedAt;
        LastReadSeq = lastReadSeq;
        IsActive = isActive;
    }

    public static ParticipantInfo Create(Guid aliasId, string displayName)
    {
        if (aliasId == Guid.Empty)
            throw new InvalidParticipantException("Alias ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new InvalidParticipantException("Display name cannot be empty.");

        return new ParticipantInfo(
            aliasId: aliasId,
            displayName: displayName.Trim(),
            joinedAt: DateTimeOffset.UtcNow,
            lastReadSeq: 0,
            isActive: true
        );
    }

    public ParticipantInfo UpdateLastReadSeq(long seq)
    {
        return this with { LastReadSeq = seq };
    }

    public ParticipantInfo Deactivate()
    {
        return this with { IsActive = false };
    }
}