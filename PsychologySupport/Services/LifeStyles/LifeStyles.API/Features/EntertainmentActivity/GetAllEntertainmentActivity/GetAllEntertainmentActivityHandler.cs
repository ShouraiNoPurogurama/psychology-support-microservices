using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Translation;
using BuildingBlocks.Pagination;
using LifeStyles.API.Abstractions;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.EntertainmentActivity.GetAllEntertainmentActivity;

public record GetAllEntertainmentActivitiesQuery(
    int PageIndex,
    int PageSize,
    string? Search,
    IntensityLevel? IntensityLevel,
    ImpactLevel? ImpactLevel
) : IQuery<GetAllEntertainmentActivitiesResult>;

public record GetAllEntertainmentActivitiesResult(PaginatedResult<EntertainmentActivityDto> EntertainmentActivities);

public class GetAllEntertainmentActivityHandler : IQueryHandler<GetAllEntertainmentActivitiesQuery,
    GetAllEntertainmentActivitiesResult>
{
    private readonly LifeStylesDbContext _context;

    private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;
    // private readonly IRedisCache _redisCache;

    public GetAllEntertainmentActivityHandler(LifeStylesDbContext context,
        IRequestClient<GetTranslatedDataRequest> translationClient
        // IRedisCache redisCache
    )
    {
        _context = context;
        _translationClient = translationClient;
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
            .ProjectToType<EntertainmentActivityDto>()
            .ToListAsync(cancellationToken);

        var translatedActivities = await activities.TranslateEntitiesAsync(
            nameof(Models.EntertainmentActivity), _translationClient, a => a.Id.ToString(), cancellationToken, 
            a => a.Name,
            a => a.Description,
            a => a.IntensityLevel,
            a => a.ImpactLevel
            );

        var result = new PaginatedResult<EntertainmentActivityDto>(
            pageIndex,
            pageSize,
            totalCount,
            translatedActivities
        );

        // await _redisCache.SetCacheDataAsync(cacheKey, result, TimeSpan.FromMinutes(10));

        return new GetAllEntertainmentActivitiesResult(result);
    }
}