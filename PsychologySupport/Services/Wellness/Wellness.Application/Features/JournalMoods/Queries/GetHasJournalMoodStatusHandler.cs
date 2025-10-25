using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Features.JournalMoods.Queries;

namespace Wellness.Application.Features.JournalMoods.Queries;

public record GetHasJournalMoodStatusQuery(
    Guid SubjectRef
) : IQuery<GetHasJournalMoodStatusResult>;

public record GetHasJournalMoodStatusResult(bool HasMood);

internal class GetHasJournalMoodStatusHandler
    : IQueryHandler<GetHasJournalMoodStatusQuery, GetHasJournalMoodStatusResult>
{
    private readonly IWellnessDbContext _context;

    public GetHasJournalMoodStatusHandler(IWellnessDbContext context)
    {
        _context = context;
    }

    public async Task<GetHasJournalMoodStatusResult> Handle(GetHasJournalMoodStatusQuery request, CancellationToken cancellationToken)
    {
        // Lấy thời gian hiện tại theo giờ Việt Nam (UTC+7)
        var nowVn = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7));
        var startOfTodayVn = nowVn.Date; // Bắt đầu ngày hôm nay
        var endOfTodayVn = startOfTodayVn.AddDays(1).AddTicks(-1); // Kết thúc ngày hôm nay (bao gồm giây cuối)

        // Chuyển sang UTC để so sánh với DB
        var startUtc = startOfTodayVn.ToUniversalTime();
        var endUtc = endOfTodayVn.ToUniversalTime();

        var hasMood = await _context.JournalMoods
            .AsNoTracking()
            .AnyAsync(jm => jm.SubjectRef == request.SubjectRef &&
                            jm.CreatedAt >= startUtc &&
                            jm.CreatedAt <= endUtc,
                      cancellationToken);

        return new GetHasJournalMoodStatusResult(hasMood);
    }
}