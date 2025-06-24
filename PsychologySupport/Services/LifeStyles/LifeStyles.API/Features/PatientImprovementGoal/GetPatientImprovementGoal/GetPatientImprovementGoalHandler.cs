using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PatientImprovementGoal.GetPatientImprovementGoal;

public record GetPatientImprovementGoalQuery(Guid PatientProfileId, DateOnly Date)
    : IQuery<GetPatientImprovementGoalResult>;

public record GetPatientImprovementGoalResult(List<PatientImprovementGoalDto> Goals);

public class GetPatientImprovementGoalHandler(LifeStylesDbContext context)
    : IQueryHandler<GetPatientImprovementGoalQuery, GetPatientImprovementGoalResult>
{
    public async Task<GetPatientImprovementGoalResult> Handle(
        GetPatientImprovementGoalQuery request,
        CancellationToken cancellationToken)
    {
        var dateOnly = request.Date.ToDateTime(TimeOnly.MinValue).Date;

        var latestAssignedDate = await context.PatientImprovementGoals
            .Where(x => x.PatientProfileId == request.PatientProfileId &&
                        x.AssignedAt.Date <= dateOnly)
            .OrderByDescending(x => x.AssignedAt)
            .Select(x => x.AssignedAt)
            .FirstOrDefaultAsync(cancellationToken);


        if (latestAssignedDate == default)
            throw new LifeStylesNotFoundException("Patient Improvement Goal", request.PatientProfileId);

        var goals = await context.PatientImprovementGoals
            .Where(x => x.PatientProfileId == request.PatientProfileId && x.AssignedAt == latestAssignedDate)
            .Include(x => x.Goal)
            .ToListAsync(cancellationToken);

        var result = goals.Adapt<List<PatientImprovementGoalDto>>();

        return new GetPatientImprovementGoalResult(result);
    }
}
