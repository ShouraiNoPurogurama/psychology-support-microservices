using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PhysicalActivity.GetAllPhysicalActivity;

public record GetAllPhysicalActivitiesQuery(PaginationRequest PaginationRequest) : IQuery<GetAllPhysicalActivitiesResult>;

public record GetAllPhysicalActivitiesResult(IEnumerable<PhysicalActivityDto> PhysicalActivities);

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
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = request.PaginationRequest.PageIndex;

        var activities = await _context.PhysicalActivities
            .OrderBy(pa => pa.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(pa => new PhysicalActivityDto(
                pa.Id,
                pa.Name,
                pa.Description,
                pa.IntensityLevel,
                pa.ImpactLevel
            ))
            .ToListAsync(cancellationToken);

        var result = activities.Adapt<IEnumerable<PhysicalActivityDto>>();

        return new GetAllPhysicalActivitiesResult(result);
    }
}