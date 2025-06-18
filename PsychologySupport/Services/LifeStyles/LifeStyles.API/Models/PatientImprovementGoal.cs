using System.ComponentModel.DataAnnotations.Schema;

namespace LifeStyles.API.Models
{
    public class PatientImprovementGoal
    {
        public Guid PatientProfileId { get; set; }
        public Guid GoalId { get; set; }

        public DateTimeOffset AssignedAt { get; set; }

        // Navigation property
        [ForeignKey("GoalId")]
        public ImprovementGoal Goal { get; set; } = null!;

    }
}
