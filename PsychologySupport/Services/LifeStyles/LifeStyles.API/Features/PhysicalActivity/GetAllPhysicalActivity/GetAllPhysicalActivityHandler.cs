using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using LifeStyles.API.Abstractions;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PhysicalActivity.GetAllPhysicalActivity;

public record GetAllPhysicalActivitiesQuery(
    [FromQuery] int PageIndex,
    [FromQuery] int PageSize,
    [FromQuery] string? Search = null,
    [FromQuery] IntensityLevel? IntensityLevel = null, 
    [FromQuery] ImpactLevel? ImpactLevel = null ) 
: IQuery<GetAllPhysicalActivitiesResult>;

public record GetAllPhysicalActivitiesResult(PaginatedResult<PhysicalActivityDto> PhysicalActivities);

public class GetAllPhysicalActivityHandler : IQueryHandler<GetAllPhysicalActivitiesQuery, GetAllPhysicalActivitiesResult>
{
    private readonly LifeStylesDbContext _context;
    private readonly IRedisCache _redisCache;

    public GetAllPhysicalActivityHandler(LifeStylesDbContext context, IRedisCache redisCache)
    {
        _context = context;
        _redisCache = redisCache;
    }

    public async Task<GetAllPhysicalActivitiesResult> Handle(GetAllPhysicalActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize;
        var pageIndex = request.PageIndex;

        var cacheKey = $"physicalActivities:{request.Search}:{request.IntensityLevel}:{request.ImpactLevel}:page{pageIndex}:size{pageSize}";

        var cachedData = await _redisCache.GetCacheDataAsync<PaginatedResult<PhysicalActivityDto>?>(cacheKey);
        if (cachedData is not null)
        {
            return new GetAllPhysicalActivitiesResult(cachedData);
        }

        var query = _context.PhysicalActivities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(ea => ea.Name.Contains(request.Search));
        }

        if (request.IntensityLevel.HasValue)
        {
            query = query.Where(ea => ea.IntensityLevel == request.IntensityLevel.Value);
        }

        if (request.ImpactLevel.HasValue)
        {
            query = query.Where(ea => ea.ImpactLevel == request.ImpactLevel.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .OrderBy(ea => ea.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<PhysicalActivityDto>(
            pageIndex,
            pageSize,
            totalCount,
            activities.Adapt<IEnumerable<PhysicalActivityDto>>()
        );

        await _redisCache.SetCacheDataAsync(cacheKey, result, TimeSpan.FromMinutes(10));

        return new GetAllPhysicalActivitiesResult(result);
    }
}
