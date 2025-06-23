namespace BuildingBlocks.Messaging.Dtos.LifeStyles;

public record EmotionSelectionFlatDto(
    Guid SelectionId,
    Guid EmotionId,
    string EmotionName,
    int? Intensity,
    int? Rank,
    DateTimeOffset LogDate
);