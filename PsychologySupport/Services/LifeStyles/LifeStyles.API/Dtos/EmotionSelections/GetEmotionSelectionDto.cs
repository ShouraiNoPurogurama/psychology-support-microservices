using LifeStyles.API.Dtos.Emotions;

namespace LifeStyles.API.Dtos.EmotionSelections;

public record GetEmotionSelectionDto(Guid Id, EmotionDto Emotion, int? Intensity, int? Rank);