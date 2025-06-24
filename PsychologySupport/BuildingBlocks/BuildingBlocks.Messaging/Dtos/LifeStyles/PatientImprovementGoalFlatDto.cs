namespace BuildingBlocks.Messaging.Dtos.LifeStyles;

public record PatientImprovementGoalFlatDto(
    Guid GoalId,
    string GoalName,
    DateTimeOffset AssignedAt
);
