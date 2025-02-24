using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.FoodActivity.GetAllFoodActivity;

public record GetAllFoodActivitiesQuery(PaginationRequest PaginationRequest) : IQuery<GetAllFoodActivitiesResult>;

public record GetAllFoodActivitiesResult(IEnumerable<FoodActivityDto> FoodActivities);

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
        var pageIndex = request.PaginationRequest.PageIndex;

        var activities = await _context.FoodActivities
            .OrderBy(fa => fa.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(fa => new FoodActivityDto(
                fa.Id,
                fa.Name,
                fa.Description,
                fa.MealTime,
                fa.FoodNutrients.Select(fn => fn.Name),
                fa.FoodCategories.Select(fc => fc.Name),
                fa.IntensityLevel.ToString()
            ))
            .ToListAsync(cancellationToken);

        return new GetAllFoodActivitiesResult(activities);
    }
}
