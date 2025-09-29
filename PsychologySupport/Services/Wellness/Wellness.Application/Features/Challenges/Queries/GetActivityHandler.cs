using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Domain.Aggregates.Challenges.Enums;

namespace Wellness.Application.Features.Challenges.Queries;

public record GetActivitiesQuery(
    ActivityType? ActivityType,
    int PageIndex = 1,
    int PageSize = 10
) : IQuery<GetActivitiesResult>;

public record GetActivitiesResult(PaginatedResult<ActivityDto> Activities);

internal class GetActivityHandler
    : IQueryHandler<GetActivitiesQuery, GetActivitiesResult>
{
    private readonly IWellnessDbContext _context;

    public GetActivityHandler(IWellnessDbContext context)
    {
        _context = context;
    }

    public async Task<GetActivitiesResult> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Activities
            .Include(a => a.ChallengeSteps)
            .Include(a => a.ProcessHistories)
            .AsNoTracking()
            .AsQueryable();

        if (request.ActivityType.HasValue)
        {
            query = query.Where(a => a.ActivityType == request.ActivityType.Value);
        }

        // mặc định sort theo CreatedAt desc
        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<ActivityDto>()
            .ToListAsync(cancellationToken);

        var paginated = new PaginatedResult<ActivityDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            items
        );

        return new GetActivitiesResult(paginated);
    }
}
