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

        //Xóa hết mục tiêu cũ của profile
        var oldGoals = context.PatientImprovementGoals
            .Where(x => x.PatientProfileId == request.PatientProfileId);
        context.PatientImprovementGoals.RemoveRange(oldGoals);

        //Thêm mới danh sách goals từ FE
        var goals = request.GoalIds
            .Select(goalId => new Models.PatientImprovementGoal
        {
            PatientProfileId = request.PatientProfileId,
            GoalId = goalId,
            AssignedAt = currentTime
        });
        
        context.PatientImprovementGoals.AddRange(goals);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePatientImprovementGoalResult(true);
    }
}
