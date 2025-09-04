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

namespace LifeStyles.API.Features02.PhysicalActivity.GetAllPhysicalActivity;

public record GetAllPhysicalActivitiesV2Query(
    int PageIndex,
    int PageSize,
    string? Search,
    IntensityLevel? IntensityLevel,
    ImpactLevel? ImpactLevel
) : IQuery<GetAllPhysicalActivitiesV2Result>;

public record GetAllPhysicalActivitiesV2Result(PaginatedResult<PhysicalActivityDto> PhysicalActivities);

public class GetAllPhysicalActivitiesV2Handler
    : IQueryHandler<GetAllPhysicalActivitiesV2Query, GetAllPhysicalActivitiesV2Result>
{
    private readonly LifeStylesDbContext _context;
    private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

    public GetAllPhysicalActivitiesV2Handler(
        LifeStylesDbContext context,
        IRequestClient<GetTranslatedDataRequest> translationClient
    )
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetAllPhysicalActivitiesV2Result> Handle(GetAllPhysicalActivitiesV2Query request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize;
        var pageIndex = request.PageIndex;

        var query = _context.PhysicalActivities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(pa => pa.Name.Contains(request.Search));
        }

        if (request.IntensityLevel.HasValue)
        {
            query = query.Where(pa => pa.IntensityLevel == request.IntensityLevel.Value);
        }

        if (request.ImpactLevel.HasValue)
        {
            query = query.Where(pa => pa.ImpactLevel == request.ImpactLevel.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .OrderBy(pa => pa.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<PhysicalActivityDto>()
            .ToListAsync(cancellationToken);

        var translatedActivities = await activities.TranslateEntitiesAsync(
            nameof(Models.PhysicalActivity),
            _translationClient,
            a => a.Id.ToString(),
            cancellationToken,
            a => a.Name,
            a => a.Description,
            a => a.IntensityLevel,
            a => a.ImpactLevel
        );

        var result = new PaginatedResult<PhysicalActivityDto>(
            pageIndex,
            pageSize,
            totalCount,
            translatedActivities
        );

        return new GetAllPhysicalActivitiesV2Result(result);
    }
}
