namespace LifeStyles.API.Dtos
{
    public record PatientImprovementGoalDto
    (
        Guid GoalId,
        string GoalName,
        DateTimeOffset AssignedAt
    );
}
