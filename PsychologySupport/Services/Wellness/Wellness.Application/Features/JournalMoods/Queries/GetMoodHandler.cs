using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Features.JournalMoods.Dtos;

namespace Wellness.Application.Features.JournalMoods.Queries;

public record GetMoodsQuery() : IQuery<GetMoodsResult>;

public record GetMoodsResult(IEnumerable<MoodDto> Moods);

internal class GetMoodHandler
    : IQueryHandler<GetMoodsQuery, GetMoodsResult>
{
    private readonly IWellnessDbContext _context;

    public GetMoodHandler(IWellnessDbContext context)
    {
        _context = context;
    }

    public async Task<GetMoodsResult> Handle(GetMoodsQuery request, CancellationToken cancellationToken)
    {
        var moods = await _context.Moods
            .AsNoTracking()
            .Select(m => new MoodDto(
                m.Id,
                m.Name,
                m.Description
            ))
            .ToListAsync(cancellationToken);

        return new GetMoodsResult(moods);
    }
}
