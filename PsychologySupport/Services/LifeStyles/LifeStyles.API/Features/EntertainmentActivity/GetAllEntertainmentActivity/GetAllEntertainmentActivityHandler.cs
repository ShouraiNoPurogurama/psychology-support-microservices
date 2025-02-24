using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.EntertainmentActivity.GetAllEntertainmentActivity;

public record GetAllEntertainmentActivitiesQuery(PaginationRequest PaginationRequest) : IQuery<GetAllEntertainmentActivitiesResult>;

public record GetAllEntertainmentActivitiesResult(IEnumerable<EntertainmentActivityDto> EntertainmentActivities);

public class GetAllEntertainmentActivityHandler : IQueryHandler<GetAllEntertainmentActivitiesQuery, GetAllEntertainmentActivitiesResult>
{
    private readonly LifeStylesDbContext _context;

    public GetAllEntertainmentActivityHandler(LifeStylesDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllEntertainmentActivitiesResult> Handle(GetAllEntertainmentActivitiesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = request.PaginationRequest.PageIndex;

        var activities = await _context.EntertainmentActivities
            .OrderBy(ea => ea.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
             .Select(ea => new EntertainmentActivityDto(
                    ea.Id,
                    ea.Name,
                    ea.Description,
                    ea.IntensityLevel.ToString(),
                    ea.ImpactLevel.ToString()
             ))
            .ToListAsync(cancellationToken);

        var result = activities.Adapt<IEnumerable<EntertainmentActivityDto>>();

        return new GetAllEntertainmentActivitiesResult(result);
    }
}
