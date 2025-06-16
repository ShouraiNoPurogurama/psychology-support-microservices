using BuildingBlocks.Enums;

namespace LifeStyles.API.Models
{
    public class CurrentEmotion
    {
        public Guid Id { get; set; }
        public Guid PatientProfileId { get; set; }
        public DateTimeOffset LogDate { get; set; }

        public Emotion? Emotion1 { get; set; }
        public Emotion? Emotion2 { get; set; }
    }
}
