using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.TherapeuticActivity.GetALTherapeuticActivity;

public record GetAllTherapeuticActivitiesQuery([FromQuery] int PageIndex,
    [FromQuery] int PageSize,
    [FromQuery] string? Search = null, // Search by Name
    [FromQuery] IntensityLevel? IntensityLevel = null, // Filter by IntensityLevel
    [FromQuery] ImpactLevel? ImpactLevel = null) // Filter by ImpactLevel
    : IQuery<GetAllTherapeuticActivitiesResult>;

public record GetAllTherapeuticActivitiesResult(PaginatedResult<TherapeuticActivityDto> TherapeuticActivities);

public class GetAllTherapeuticActivityHandler
    : IQueryHandler<GetAllTherapeuticActivitiesQuery, GetAllTherapeuticActivitiesResult>
{
    private readonly LifeStylesDbContext _context;

    public GetAllTherapeuticActivityHandler(LifeStylesDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllTherapeuticActivitiesResult> Handle(
        GetAllTherapeuticActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize;
        var pageIndex = request.PageIndex;

        var query = _context.TherapeuticActivities.AsQueryable();

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

        var result = new PaginatedResult<TherapeuticActivityDto>(
           pageIndex,
           pageSize,
           totalCount,
           activities.Adapt<IEnumerable<TherapeuticActivityDto>>()
       );

        return new GetAllTherapeuticActivitiesResult(result);
    }
}