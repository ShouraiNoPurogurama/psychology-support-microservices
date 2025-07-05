namespace LifeStyles.API.Models
{
    public class PatientEmotionCheckpoint
    {
        public Guid Id { get; set; }
        public Guid PatientProfileId { get; set; }
        public DateTimeOffset LogDate { get; set; }
        public ICollection<EmotionSelection> EmotionSelections { get; set; } = new List<EmotionSelection>();
    }
}