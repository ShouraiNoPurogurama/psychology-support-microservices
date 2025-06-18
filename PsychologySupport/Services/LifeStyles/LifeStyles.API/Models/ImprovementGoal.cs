namespace LifeStyles.API.Models
{
    public class ImprovementGoal
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ICollection<PatientImprovementGoal> PatientImprovementGoals { get; set; } = [];
    }
}
