namespace LifeStyles.API.Dtos.EmotionSelections;

public record CreateEmotionSelectionDto(
    Guid EmotionId,
    int? Intensity = null,
    int? Rank = null);