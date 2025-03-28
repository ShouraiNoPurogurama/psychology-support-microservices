using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.FoodActivity.GetAllFoodActivity;

public record GetAllFoodActivitiesQuery(
     [FromQuery] int PageIndex,
     [FromQuery] int PageSize,
     [FromQuery] string? Search = null // Search by FoodActivity.Name or FoodCategory.Name OR FoodNutries.Name
) : IQuery<GetAllFoodActivitiesResult>;

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
        var pageSize = request.PageSize;
        var pageIndex = request.PageIndex;

        var query = _context.FoodActivities
            .Include(fa => fa.FoodNutrients)
            .Include(fa => fa.FoodCategories)
            .AsQueryable();

        // Search by FoodActivity.Name or FoodCategory.Name
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(fa =>
                fa.Name.Contains(request.Search) ||
                fa.FoodCategories.Any(fc => fc.Name.Contains(request.Search)) ||
                fa.FoodNutrients.Any(fn => fn.Name.Contains(request.Search))
            );
        }


        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .OrderBy(fa => fa.Name)
            .Skip((pageIndex - 1) * pageSize) 
            .Take(pageSize)
            .ToListAsync(cancellationToken);

     
        var result = new PaginatedResult<FoodActivityDto>(
            pageIndex,
            pageSize,
            totalCount,
            activities.Adapt<IEnumerable<FoodActivityDto>>() 
        );

        return new GetAllFoodActivitiesResult(result);
    }
}