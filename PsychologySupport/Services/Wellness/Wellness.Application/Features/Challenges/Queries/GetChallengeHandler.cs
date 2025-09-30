using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Domain.Aggregates.Challenges.Enums;

namespace Wellness.Application.Features.Challenges.Queries;

public record GetChallengesQuery(
    ChallengeType? ChallengeType,
    int PageIndex = 1,
    int PageSize = 10
) : IQuery<GetChallengesResult>;

public record GetChallengesResult(PaginatedResult<ChallengeDto> Challenges);

internal class GetChallengesHandler
    : IQueryHandler<GetChallengesQuery, GetChallengesResult>
{
    private readonly IWellnessDbContext _context;

    public GetChallengesHandler(IWellnessDbContext context)
    {
        _context = context;
    }

    public async Task<GetChallengesResult> Handle(GetChallengesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Challenges
            .Include(c => c.ChallengeSteps)
                .ThenInclude(s => s.Activity)
            .AsNoTracking()
            .AsQueryable();

        if (request.ChallengeType.HasValue)
        {
            query = query.Where(c => c.ChallengeType == request.ChallengeType.Value);
        }

        // mặc định sort theo CreatedAt desc
        query = query.OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<ChallengeDto>()
            .ToListAsync(cancellationToken);

        var paginated = new PaginatedResult<ChallengeDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            items
        );

        return new GetChallengesResult(paginated);
    }
}
