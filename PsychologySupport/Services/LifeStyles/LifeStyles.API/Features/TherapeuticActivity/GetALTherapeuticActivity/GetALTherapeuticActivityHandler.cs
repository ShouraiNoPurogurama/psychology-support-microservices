using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using LifeStyles.API.Dtos;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Features.TherapeuticActivity.GetAllTherapeuticActivity;

public record GetAllTherapeuticActivitiesQuery(PaginationRequest PaginationRequest) : IQuery<GetAllTherapeuticActivitiesResult>;

public record GetAllTherapeuticActivitiesResult(IEnumerable<TherapeuticActivityDto> TherapeuticActivities);

public class GetAllTherapeuticActivityHandler : IQueryHandler<GetAllTherapeuticActivitiesQuery, GetAllTherapeuticActivitiesResult>
{
    private readonly LifeStylesDbContext _context;

    public GetAllTherapeuticActivityHandler(LifeStylesDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllTherapeuticActivitiesResult> Handle(GetAllTherapeuticActivitiesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PaginationRequest.PageSize;
        var pageIndex = request.PaginationRequest.PageIndex;

        var activities = await _context.TherapeuticActivities
            .Include(ta => ta.TherapeuticType) 
            .OrderBy(ta => ta.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(ta => new TherapeuticActivityDto(
                ta.Id,
                ta.TherapeuticType.Name, 
                ta.Name,
                ta.Description,
                ta.Instructions,
                ta.IntensityLevel.ToString(),
                ta.ImpactLevel.ToString()
            ))
            .ToListAsync(cancellationToken);

        return new GetAllTherapeuticActivitiesResult(activities);
    }
}
