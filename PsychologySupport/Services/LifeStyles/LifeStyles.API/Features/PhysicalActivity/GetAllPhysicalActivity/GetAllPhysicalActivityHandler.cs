using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PhysicalActivity.GetAllPhysicalActivity;


public record GetAllPhysicalActivitiesQuery(
    [FromQuery] int PageIndex,
    [FromQuery] int PageSize,
    [FromQuery] string? Search = null, // Search by Name
    [FromQuery] IntensityLevel? IntensityLevel = null, // Filter by IntensityLevel
    [FromQuery] ImpactLevel? ImpactLevel = null ) // Filter by ImpactLevel)
: IQuery<GetAllPhysicalActivitiesResult>;

public record GetAllPhysicalActivitiesResult(PaginatedResult<PhysicalActivityDto> PhysicalActivities);

public class GetAllPhysicalActivityHandler : IQueryHandler<GetAllPhysicalActivitiesQuery, GetAllPhysicalActivitiesResult>
{
    private readonly LifeStylesDbContext _context;

    public GetAllPhysicalActivityHandler(LifeStylesDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllPhysicalActivitiesResult> Handle(GetAllPhysicalActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize;
        var pageIndex = request.PageIndex;

        var query = _context.PhysicalActivities.AsQueryable();

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

        var result = new PaginatedResult<PhysicalActivityDto>(
           pageIndex,
           pageSize,
           totalCount,
           activities.Adapt<IEnumerable<PhysicalActivityDto>>()
       );

        return new GetAllPhysicalActivitiesResult(result);
    }
}