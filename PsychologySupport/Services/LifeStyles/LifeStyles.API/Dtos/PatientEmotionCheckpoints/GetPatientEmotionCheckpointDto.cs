using LifeStyles.API.Dtos.EmotionSelections;

namespace LifeStyles.API.Dtos.PatientEmotionCheckpoints;

public record PatientEmotionCheckpointDto (Guid Id,
    List<GetEmotionSelectionDto> EmotionSelections,
    DateTimeOffset LogDate);