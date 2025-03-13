using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.FoodActivity.GetAllFoodActivity;
public record GetAllFoodActivitiesQuery(PaginationRequest PaginationRequest) : IQuery<GetAllFoodActivitiesResult>;

public record GetAllFoodActivitiesResult(PaginatedResult<FoodActivityDto> FoodActivities);

public class GetAllFoodActivityHandler : IQueryHandler<GetAllFoodActivitiesQuery, GetAllFoodActivitiesResult>
{
    private readonly LifeStylesDbContext _context;

    public GetAllFoodActivityHandler(LifeStylesDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllFoodActivitiesResult> Handle(GetAllFoodActivitiesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = Math.Max(0, request.PaginationRequest.PageIndex - 1);

        var query = _context.FoodActivities
            .OrderBy(fa => fa.Name)
            .Select(fa => new FoodActivityDto(
                fa.Id,
                fa.Name,
                fa.Description,
                fa.MealTime,
                fa.FoodNutrients.Select(fn => fn.Name),
                fa.FoodCategories.Select(fc => fc.Name),
                fa.IntensityLevel
            ));

        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<FoodActivityDto>(
            pageIndex + 1,
            pageSize,
            totalCount,
            activities
        );

        return new GetAllFoodActivitiesResult(paginatedResult);
    }
}