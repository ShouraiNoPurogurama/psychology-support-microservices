using Wellness.Domain.Abstractions;
using Wellness.Domain.Enums;

namespace Wellness.Domain.Events;

public static class ChallengeDomainEvents
{
    public record ChallengeProgressCreatedEvent(
        Guid ChallengeProgressId,
        Guid SubjectRef,
        Guid ChallengeId,
        List<Guid> StepIds 
    ) : IDomainEvent;

    public record ChallengeProgressUpdatedEvent(
       Guid ChallengeProgressId,
       Guid SubjectRef,
       Guid ChallengeStepId,
       Guid ActivityId,           
       Guid? PostMoodId,
       ProcessStatus Status
   ) : IDomainEvent;

}
