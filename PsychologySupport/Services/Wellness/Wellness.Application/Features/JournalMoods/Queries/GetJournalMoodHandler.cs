using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Features.JournalMoods.Dtos;

namespace Wellness.Application.Features.JournalMoods.Queries;

public record GetJournalMoodsQuery(
    Guid SubjectRef,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null
) : IQuery<GetJournalMoodsResult>;

public record GetJournalMoodsResult(IReadOnlyList<JournalMoodDto> Moods);

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
            .Include(jm => jm.Mood) 
            .Where(jm => jm.SubjectRef == request.SubjectRef);

        // Lọc theo thời gian (chuyển giờ Việt Nam -> UTC để so với DB)
        if (request.StartDate.HasValue)
        {
            var startUtc = request.StartDate.Value.ToUniversalTime();
            query = query.Where(jm => jm.CreatedAt >= startUtc);
        }

        if (request.EndDate.HasValue)
        {
            var endUtc = request.EndDate.Value.ToUniversalTime();
            query = query.Where(jm => jm.CreatedAt <= endUtc);
        }

        var items = await query
            .OrderByDescending(jm => jm.CreatedAt)
            .Select(jm => new JournalMoodDto(
                jm.Id,
                jm.SubjectRef,
                jm.Mood.Name,         
                jm.Mood.IconCode,
                jm.Note,
                jm.CreatedAt.ToOffset(TimeSpan.FromHours(7))
            ))
            .ToListAsync(cancellationToken);

        return new GetJournalMoodsResult(items);
    }
}
