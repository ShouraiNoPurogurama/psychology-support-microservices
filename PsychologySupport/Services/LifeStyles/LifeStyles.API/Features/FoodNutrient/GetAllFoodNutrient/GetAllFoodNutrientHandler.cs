using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Abstractions;
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
    private readonly IRedisCache _redisCache;

    public GetAllFoodNutrientHandler(LifeStylesDbContext context, IRedisCache redisCache)
    {
        _context = context;
        _redisCache = redisCache;
    }

    public async Task<GetAllFoodNutrientResult> Handle(
        GetAllFoodNutrientQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = Math.Max(0, request.PaginationRequest.PageIndex - 1);

        var cacheKey = $"foodNutrients:page{pageIndex + 1}:size{pageSize}";

        var cachedData = await _redisCache.GetCacheDataAsync<PaginatedResult<FoodNutrientDto>?>(cacheKey);
        if (cachedData is not null)
        {
            return new GetAllFoodNutrientResult(cachedData);
        }

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

        await _redisCache.SetCacheDataAsync(cacheKey, paginatedResult, TimeSpan.FromMinutes(10));

        return new GetAllFoodNutrientResult(paginatedResult);
    }
}
