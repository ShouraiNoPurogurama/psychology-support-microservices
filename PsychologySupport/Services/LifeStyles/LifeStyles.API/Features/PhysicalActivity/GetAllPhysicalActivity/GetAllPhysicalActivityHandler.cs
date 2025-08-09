using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Translation;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PhysicalActivity.GetAllPhysicalActivity;

public record GetAllPhysicalActivitiesQuery(
    int PageIndex,
    int PageSize,
    string? Search,
    IntensityLevel? IntensityLevel,
    ImpactLevel? ImpactLevel
) : IQuery<GetAllPhysicalActivitiesResult>;

public record GetAllPhysicalActivitiesResult(PaginatedResult<PhysicalActivityDto> PhysicalActivities);

public class GetAllPhysicalActivityHandler : IQueryHandler<GetAllPhysicalActivitiesQuery, GetAllPhysicalActivitiesResult>
{
    private readonly LifeStylesDbContext _context;
    private readonly IRequestClient<GetTranslatedDataRequest> _translationClient;

    public GetAllPhysicalActivityHandler(
        LifeStylesDbContext context,
        IRequestClient<GetTranslatedDataRequest> translationClient
    )
    {
        _context = context;
        _translationClient = translationClient;
    }

    public async Task<GetAllPhysicalActivitiesResult> Handle(GetAllPhysicalActivitiesQuery request,
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
            nameof(Models.PhysicalActivity), _translationClient, a => a.Id.ToString(), cancellationToken,
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

        return new GetAllPhysicalActivitiesResult(result);
    }
}
