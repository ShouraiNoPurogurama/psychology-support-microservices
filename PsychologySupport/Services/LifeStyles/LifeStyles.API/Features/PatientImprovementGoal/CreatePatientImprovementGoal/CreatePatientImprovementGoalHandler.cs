using BuildingBlocks.CQRS;
using LifeStyles.API.Data;

namespace LifeStyles.API.Features.PatientImprovementGoal.CreatePatientImprovementGoal;

public record CreatePatientImprovementGoalCommand(
    Guid PatientProfileId,
    List<Guid> GoalIds
) : ICommand<CreatePatientImprovementGoalResult>;

public record CreatePatientImprovementGoalResult(bool IsSucceeded);
public class CreatePatientImprovementGoalHandler(LifeStylesDbContext context)
    : ICommandHandler<CreatePatientImprovementGoalCommand, CreatePatientImprovementGoalResult>
{
    public async Task<CreatePatientImprovementGoalResult> Handle(
        CreatePatientImprovementGoalCommand request,
        CancellationToken cancellationToken)
    {
        var currentTime = DateTimeOffset.UtcNow;

        var goals = request.GoalIds.Select(goalId => new Models.PatientImprovementGoal
        {
            PatientProfileId = request.PatientProfileId,
            GoalId = goalId,
            AssignedAt = currentTime
        });

        await context.PatientImprovementGoals.AddRangeAsync(goals, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePatientImprovementGoalResult(true);
    }
}
