using Wellness.Domain.Abstractions;
using Wellness.Domain.Enums;

namespace Wellness.Domain.Events;

public static class ChallengeDomainEvents
{
    public record ChallengeProgressCreatedEvent(
        Guid ChallengeProgressId,
        Guid SubjectRef,
        Guid ChallengeId,
        List<Guid> StepIds // danh sách step thuộc challenge
    ) : IDomainEvent;

    public record ChallengeProgressUpdatedEvent(
        Guid ChallengeProgressId,
        Guid ChallengeStepId,
        Guid? PostMoodId,
        ProcessStatus Status
    ) : IDomainEvent;
}
