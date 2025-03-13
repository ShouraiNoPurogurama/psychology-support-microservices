using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.TherapeuticActivity.GetALTherapeuticActivity;

public record GetAllTherapeuticActivitiesQuery(PaginationRequest PaginationRequest)
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
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = Math.Max(0, request.PaginationRequest.PageIndex - 1);

        var query = _context.TherapeuticActivities
            .Include(ta => ta.TherapeuticType)
            .OrderBy(ta => ta.Name)
            .Select(ta => new TherapeuticActivityDto(
                ta.Id,
                ta.TherapeuticType.Name,
                ta.Name,
                ta.Description,
                ta.Instructions,
                ta.IntensityLevel,
                ta.ImpactLevel
            ));

        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<TherapeuticActivityDto>(
            pageIndex + 1,
            pageSize,
            totalCount,
            activities
        );

        return new GetAllTherapeuticActivitiesResult(paginatedResult);
    }
}