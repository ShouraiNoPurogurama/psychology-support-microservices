using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Translation;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features02.EntertainmentActivity.GetAllEntertainmentActivity;

public record GetAllEntertainmentActivitiesV2Query(
    int PageIndex,
    int PageSize,
    string? Search,
    IntensityLevel? IntensityLevel,
    ImpactLevel? ImpactLevel
) : IQuery<GetAllEntertainmentActivitiesV2Result>;

public record GetAllEntertainmentActivitiesV2Result(
    PaginatedResult<EntertainmentActivityDto> EntertainmentActivities
);

public class GetAllEntertainmentActivityV2Handler
    : IQueryHandler<GetAllEntertainmentActivitiesV2Query, GetAllEntertainmentActivitiesV2Result>
{
    private readonly LifeStylesDbContext _context;
    private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

    public GetAllEntertainmentActivityV2Handler(
        LifeStylesDbContext context,
        IRequestClient<GetTranslatedDataRequest> translationClient
    )
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetAllEntertainmentActivitiesV2Result> Handle(
        GetAllEntertainmentActivitiesV2Query request,
        CancellationToken cancellationToken)
    {
        var query = _context.EntertainmentActivities.AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(ea => ea.Name.Contains(request.Search));

        // Filter by IntensityLevel
        if (request.IntensityLevel.HasValue)
            query = query.Where(ea => ea.IntensityLevel == request.IntensityLevel.Value);

        // Filter by ImpactLevel
        if (request.ImpactLevel.HasValue)
            query = query.Where(ea => ea.ImpactLevel == request.ImpactLevel.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .OrderBy(ea => ea.Name)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<EntertainmentActivityDto>()
            .ToListAsync(cancellationToken);

        // Translate
        var translatedActivities = await activities.TranslateEntitiesAsync(
            nameof(Models.EntertainmentActivity),
            _translationClient,
            a => a.Id.ToString(),
            cancellationToken,
            a => a.Name,
            a => a.Description,
            a => a.IntensityLevel,
            a => a.ImpactLevel
        );

        var result = new PaginatedResult<EntertainmentActivityDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            translatedActivities
        );

        return new GetAllEntertainmentActivitiesV2Result(result);
    }
}
