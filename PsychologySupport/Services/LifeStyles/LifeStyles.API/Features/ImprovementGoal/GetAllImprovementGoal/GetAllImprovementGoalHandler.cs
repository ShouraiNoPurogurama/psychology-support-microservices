using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.ImprovementGoal.GetAllImprovementGoal;

public record GetAllImprovementGoalQuery() : IQuery<GetAllImprovementGoalResult>;

public record GetAllImprovementGoalResult(IEnumerable<ImprovementGoalDto> Goals);

public class GetAllImprovementGoalHandler(LifeStylesDbContext context)
    : IQueryHandler<GetAllImprovementGoalQuery, GetAllImprovementGoalResult>
{
    public async Task<GetAllImprovementGoalResult> Handle(GetAllImprovementGoalQuery request, CancellationToken cancellationToken)
    {
        var goals = await context.ImprovementGoals.ToListAsync(cancellationToken);

        var goalDtos = goals.Adapt<IEnumerable<ImprovementGoalDto>>();

        return new GetAllImprovementGoalResult(goalDtos);
    }
}
