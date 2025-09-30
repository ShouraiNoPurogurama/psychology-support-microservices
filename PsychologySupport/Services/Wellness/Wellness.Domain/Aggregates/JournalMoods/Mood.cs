using Wellness.Domain.Aggregates.Challenges;
using Wellness.Domain.Aggregates.ProcessHistories;

namespace Wellness.Domain.Aggregates.JournalMoods
{
    public class Mood
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;         
        public string? Description { get; set; }          
        public string? IconCode { get; set; }                       
        public int Value { get; set; }          // Giá trị 1 đến 7

        public virtual ICollection<JournalMood> JournalMoods { get; set; } = new List<JournalMood>();
        public virtual ICollection<ProcessHistory> ProcessHistories { get; set; } = new List<ProcessHistory>();
        public virtual ICollection<ChallengeStepProgress> ChallengeStepProgresses { get; set; } = new List<ChallengeStepProgress>();
    }
}
