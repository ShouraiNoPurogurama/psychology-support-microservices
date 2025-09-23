using Wellness.Domain.Aggregates.ProcessHistory;
using Wellness.Domain.Abstractions;
using Wellness.Domain.Models;
using Wellness.Domain.Aggregates.Challenge.Enums;

namespace Wellness.Domain.Aggregates.Challenge;

public partial class Activity : AuditableEntity<Guid>
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ActivityType ActivityType { get; set; }

    public int? Duration { get; set; }

    public string? Instructions { get; set; }

    public virtual ICollection<ChallengeStep> ChallengeSteps { get; set; } = new List<ChallengeStep>();

    public virtual ICollection<ProcessHistory.ProcessHistory> ProcessHistories { get; set; } = new List<ProcessHistory.ProcessHistory>();
}
