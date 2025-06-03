using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using LifeStyles.API.Abstractions;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.EntertainmentActivity.GetAllEntertainmentActivity;

public record GetAllEntertainmentActivitiesQuery(
    [FromQuery] int PageIndex = 1,
    [FromQuery] int PageSize = 10,
    [FromQuery] string? Search = null, // Search by Name
    [FromQuery] IntensityLevel? IntensityLevel = null, // Filter by IntensityLevel
    [FromQuery] ImpactLevel? ImpactLevel = null // Filter by ImpactLevel
) : IQuery<GetAllEntertainmentActivitiesResult>;

public record GetAllEntertainmentActivitiesResult(PaginatedResult<EntertainmentActivityDto> EntertainmentActivities);

public class GetAllEntertainmentActivityHandler : IQueryHandler<GetAllEntertainmentActivitiesQuery,
    GetAllEntertainmentActivitiesResult>
{
    private readonly LifeStylesDbContext _context;
    // private readonly IRedisCache _redisCache;

    public GetAllEntertainmentActivityHandler(LifeStylesDbContext context
        // IRedisCache redisCache
        )
    {
        _context = context;
        // _redisCache = redisCache;
    }

    public async Task<GetAllEntertainmentActivitiesResult> Handle(GetAllEntertainmentActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize;
        var pageIndex = request.PageIndex;
        
        // var cacheKey = $"entertainmentActivities:{request.Search}:{request.IntensityLevel}:{request.ImpactLevel}:page{pageIndex}:size{pageSize}";

        // var cachedData = await _redisCache.GetCacheDataAsync<PaginatedResult<EntertainmentActivityDto>?>(cacheKey);
        
        // if (cachedData is not null)
        // {
        //     return new GetAllEntertainmentActivitiesResult(cachedData);
        // }
        
        var query = _context.EntertainmentActivities.AsQueryable();

        // Search by Name
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(ea => ea.Name.Contains(request.Search));
        }

        // Filter by IntensityLevel
        if (request.IntensityLevel.HasValue)
        {
            query = query.Where(ea => ea.IntensityLevel == request.IntensityLevel.Value);
        }

        // Filter by ImpactLevel
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

        var result = new PaginatedResult<EntertainmentActivityDto>(
            pageIndex,
            pageSize,
            totalCount,
            activities.Adapt<IEnumerable<EntertainmentActivityDto>>()
        );
        
        // await _redisCache.SetCacheDataAsync(cacheKey, result, TimeSpan.FromMinutes(10));

        return new GetAllEntertainmentActivitiesResult(result);
    }
}