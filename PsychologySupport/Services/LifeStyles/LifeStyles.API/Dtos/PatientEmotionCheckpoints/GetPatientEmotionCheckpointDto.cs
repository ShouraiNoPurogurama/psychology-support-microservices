using LifeStyles.API.Dtos.EmotionSelections;

namespace LifeStyles.API.Dtos.PatientEmotionCheckpoints;

public record PatientEmotionCheckpointDto (Guid Id,
    List<GetEmotionSelectionDto> Emotions,
    DateTimeOffset LogDate);