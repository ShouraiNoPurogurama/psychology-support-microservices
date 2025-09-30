using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wellness.Domain.Enums;

namespace Wellness.Application.Features.Challenges.Dtos
{
    public record ChallengeProgressDto
    {
        public Guid Id { get; init; }
        public Guid SubjectRef { get; init; }
        public Guid ChallengeId { get; init; }
        public string ChallengeTitle { get; init; } = string.Empty;
        public string? ChallengeDescription { get; init; }
        public string ChallengeType { get; init; } = string.Empty;
        public ProcessStatus ProcessStatus { get; init; }
        public int ProgressPercent { get; init; }
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset? EndDate { get; init; }

        public List<ChallengeStepProgressDto> Steps { get; init; } = new();
    }

    public record ChallengeStepProgressDto
    {
        public Guid StepId { get; init; }
        public int DayNumber { get; init; }
        public int OrderIndex { get; init; }
        public ActivityDto? Activity { get; init; }
        public ProcessStatus ProcessStatus { get; init; }
        public DateTimeOffset? StartedAt { get; init; }
        public DateTimeOffset? CompletedAt { get; init; }
        public Guid? PostMoodId { get; init; }
    }
}
