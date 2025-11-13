using Wellness.Domain.Abstractions;
using Wellness.Domain.Aggregates.Challenges.Enums;
using Wellness.Domain.Aggregates.ProcessHistories;
using Wellness.Domain.Enums;

namespace Wellness.Domain.Aggregates.Challenges;

public partial class Challenge : AuditableEntity<Guid>
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public ChallengeType ChallengeType { get; set; }

    public int DurationActivity { get; set; } // Số lượng hoạt động trong thử thách

    public int DurationDate { get; set; } // Số ngày của thử thách

    public ImprovementTag ImprovementTag { get; set; }

    public TagScope Scope { get; set; }

    public Guid? MediaId { get; set; } 

    //public Guid? ModuleId { get; set; }

    public virtual ICollection<ChallengeProgress> ChallengeProgresses { get; set; } = new List<ChallengeProgress>();

    public virtual ICollection<ChallengeStep> ChallengeSteps { get; set; } = new List<ChallengeStep>();

    public virtual ICollection<ProcessHistory> ProcessHistories { get; set; } = new List<ProcessHistory>();
}
