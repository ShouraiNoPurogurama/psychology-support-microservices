using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Features.JournalMoods.Dtos;

namespace Wellness.Application.Features.JournalMoods.Queries;

public record GetJournalMoodsQuery(
    Guid SubjectRef,
    int PageIndex = 1,
    int PageSize = 10,
    string SortDirection = "desc" // "asc" hoặc "desc"
) : IQuery<GetJournalMoodsResult>;

public record GetJournalMoodsResult(PaginatedResult<JournalMoodDto> Moods);

internal class GetJournalMoodHandler
    : IQueryHandler<GetJournalMoodsQuery, GetJournalMoodsResult>
{
    private readonly IWellnessDbContext _context;

    public GetJournalMoodHandler(IWellnessDbContext context)
    {
        _context = context;
    }

    public async Task<GetJournalMoodsResult> Handle(GetJournalMoodsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.JournalMoods
            .AsNoTracking()
            .Where(jm => jm.SubjectRef == request.SubjectRef);

        var sortDesc = request.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        query = sortDesc
            ? query.OrderByDescending(jm => jm.CreatedAt)
            : query.OrderBy(jm => jm.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(jm => new JournalMoodDto(
                jm.Id,
                jm.SubjectRef,
                jm.MoodId,
                jm.Note,
                jm.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var paginated = new PaginatedResult<JournalMoodDto>(
            request.PageIndex,
            request.PageSize,
            totalCount,
            items
        );

        return new GetJournalMoodsResult(paginated);
    }
}
