using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Translation;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features02.ImprovementGoal.GetAllImprovementGoal;

public record GetAllImprovementGoalQuery() : IQuery<GetAllImprovementGoalResult>;

public record GetAllImprovementGoalResult(IEnumerable<ImprovementGoalDto> Goals);

public class GetAllImprovementGoalHandler(
    LifeStylesDbContext context,
    IRequestClient<GetTranslatedDataRequest> translatedDataClient)
    : IQueryHandler<GetAllImprovementGoalQuery, GetAllImprovementGoalResult>
{
    public async Task<GetAllImprovementGoalResult> Handle(GetAllImprovementGoalQuery request, CancellationToken cancellationToken)
    {
        var goals = await context.ImprovementGoals.ToListAsync(cancellationToken);

        var goalDtos = goals.Adapt<List<ImprovementGoalDto>>();

        var translatedGoals = await goalDtos.TranslateEntitiesAsync(nameof(Models.ImprovementGoal), translatedDataClient,
            i => i.Id.ToString(), cancellationToken, 
            i => i.Name, 
            i => i.Description);

        return new GetAllImprovementGoalResult(translatedGoals);
    }
}