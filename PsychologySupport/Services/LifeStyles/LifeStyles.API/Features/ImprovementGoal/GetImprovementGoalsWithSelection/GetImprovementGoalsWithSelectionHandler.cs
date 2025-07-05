using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.ImprovementGoal.GetImprovementGoalsWithSelection;

public record GetImprovementGoalsWithSelectionQuery(Guid ProfileId, PaginationRequest PaginationRequest)
    : IQuery<GetImprovementGoalsWithSelectionResult>;

public record GetImprovementGoalsWithSelectionResult(PaginatedResult<ImprovementGoalWithSelectionDto> Goals);

public class GetImprovementGoalsWithSelectionHandler(
    LifeStylesDbContext context,
    IRequestClient<GetTranslatedDataRequest> translatedDataClient)
    : IQueryHandler<GetImprovementGoalsWithSelectionQuery, GetImprovementGoalsWithSelectionResult>
{
    public async Task<GetImprovementGoalsWithSelectionResult> Handle(
        GetImprovementGoalsWithSelectionQuery request, CancellationToken cancellationToken)
    {
        var pageIndex = request.PaginationRequest.PageIndex;
        var pageSize = request.PaginationRequest.PageSize;

        var query = context.ImprovementGoals;
        
        var allGoals = await query
            .Select(goal => new ImprovementGoalWithSelectionDto(
                goal.Id,
                goal.Name,
                goal.Description,
                context.PatientImprovementGoals.Any(pig => pig.PatientProfileId == request.ProfileId && pig.GoalId == goal.Id)
            ))
            .ToListAsync(cancellationToken);

        var translatedGoals = await allGoals
            .TranslateEntitiesAsync(nameof(Models.ImprovementGoal),
                translatedDataClient,
                i => i.Id.ToString(),
                cancellationToken,
                i => i.Description,
                i => i.Name);

        var orderedGoals = translatedGoals
            .OrderByDescending(e => e.IsSelected)
            .ThenBy(i => i.Name).ToList();

        var paginatedGoals = orderedGoals
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PaginatedResult<ImprovementGoalWithSelectionDto>(
            pageIndex,
            pageSize,
            TotalCount: allGoals.Count,
            paginatedGoals
        );

        return new GetImprovementGoalsWithSelectionResult(result);
    }
}