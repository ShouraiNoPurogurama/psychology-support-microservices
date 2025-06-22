namespace LifeStyles.API.Models;

public class EmotionSelection
{
    public Guid Id { get; set; }
    public Guid EmotionCheckpointId { get; set; }
    public Guid EmotionId { get; set; }
    public int? Intensity { get; set; }
    public int? Rank { get; set; }
    public Emotion Emotion { get; set; } = null!;
    public PatientEmotionCheckpoint EmotionCheckpoint { get; set; } = null!;
}
