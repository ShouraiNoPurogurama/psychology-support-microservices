using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.PhysicalActivity.GetAllPhysicalActivity;


public record GetAllPhysicalActivitiesQuery(PaginationRequest PaginationRequest)
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
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = Math.Max(0, request.PaginationRequest.PageIndex - 1);

        var query = _context.PhysicalActivities
            .OrderBy(pa => pa.Name)
            .Select(pa => new PhysicalActivityDto(
                pa.Id,
                pa.Name,
                pa.Description,
                pa.IntensityLevel,
                pa.ImpactLevel
            ));

        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var paginatedResult = new PaginatedResult<PhysicalActivityDto>(
            pageIndex + 1,
            pageSize,
            totalCount,
            activities
        );

        return new GetAllPhysicalActivitiesResult(paginatedResult);
    }
}