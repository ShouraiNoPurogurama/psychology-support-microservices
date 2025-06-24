using BuildingBlocks.Messaging.Dtos.LifeStyles;

namespace BuildingBlocks.Messaging.Events.LifeStyle;

public record AggregatePatientLifestyleResponse(
    Guid PatientProfileId,
    List<PatientImprovementGoalFlatDto> ImprovementGoals,
    List<EmotionSelectionFlatDto> EmotionSelections
);
