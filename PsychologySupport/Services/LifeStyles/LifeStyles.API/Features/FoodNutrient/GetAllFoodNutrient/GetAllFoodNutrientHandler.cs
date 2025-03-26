using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.FoodNutrient.GetAllFoodNutrient;

public record GetAllFoodNutrientQuery(PaginationRequest PaginationRequest)
    : IQuery<GetAllFoodNutrientResult>;

public record GetAllFoodNutrientResult(PaginatedResult<FoodNutrientDto> FoodNutrients);

public class GetAllFoodNutrientHandler
    : IQueryHandler<GetAllFoodNutrientQuery, GetAllFoodNutrientResult>
{
    private readonly LifeStylesDbContext _context;

    public GetAllFoodNutrientHandler(LifeStylesDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllFoodNutrientResult> Handle(
        GetAllFoodNutrientQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = Math.Max(0, request.PaginationRequest.PageIndex - 1);

        var query = _context.FoodNutrients
            .OrderBy(fn => fn.Name)
            .Select(fn => new FoodNutrientDto(
                fn.Id,
                fn.Name,
                fn.Description
            ));

        var totalCount = await query.CountAsync(cancellationToken);

        var nutrients = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<FoodNutrientDto>(
            pageIndex + 1,
            pageSize,
            totalCount,
            nutrients
        );

        return new GetAllFoodNutrientResult(paginatedResult);
    }
}
