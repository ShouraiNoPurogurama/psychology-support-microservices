using Microsoft.EntityFrameworkCore;
using Profile.API.Data.Pii;
using Profile.API.Domains.Pii.Dtos;

namespace Profile.API.Domains.Pii.Features.GetDailyNewUsers;

public record GetDailyNewUsersQuery(
    int Year,
    int Month
) : IQuery<GetDailyNewUsersResult>;

public record GetDailyNewUsersResult(DailyNewUserStatsDto? DailyNewUserStats);

public class GetDailyNewUsersHandler(
    ILogger<GetDailyNewUsersHandler> logger,
    PiiDbContext dbContext
) : IQueryHandler<GetDailyNewUsersQuery, GetDailyNewUsersResult>
{
    public async Task<GetDailyNewUsersResult> Handle(
        GetDailyNewUsersQuery request,
        CancellationToken cancellationToken)
    {
        var start = new DateOnly(request.Year, request.Month, 1);
        var end = start.AddMonths(1);

        var startDateTimeUtc = DateTime.SpecifyKind(
            start.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc
        );
        var endDateTimeUtc = DateTime.SpecifyKind(
            end.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc
        );

        var startOffset = new DateTimeOffset(startDateTimeUtc);
        var endOffset = new DateTimeOffset(endDateTimeUtc);

        // Query trong DB: chỉ lấy những ngày có user
        var rawPoints = await dbContext.PersonProfiles
            .Where(u => u.CreatedAt >= startOffset && u.CreatedAt < endOffset)
            .GroupBy(u => new
            {
                u.CreatedAt.UtcDateTime.Year,
                u.CreatedAt.UtcDateTime.Month,
                u.CreatedAt.UtcDateTime.Day
            })
            .Select(g => new DailyNewUserPointDto(
                new DateOnly(g.Key.Year, g.Key.Month, g.Key.Day),
                g.Count()
            ))
            .ToListAsync(cancellationToken);

        // Map về dictionary cho dễ lookup
        var dict = rawPoints.ToDictionary(x => x.Date, x => x.NewUserCount);

        // Generate full list ngày trong tháng (LEFT JOIN bằng não)
        var allDays = Enumerable
            .Range(0, end.DayNumber - start.DayNumber)
            .Select(offset => start.AddDays(offset));

        var normalizedPoints = allDays
            .Select(d => new DailyNewUserPointDto(
                d,
                dict.TryGetValue(d, out var count) ? count : 0
            ))
            .OrderBy(x => x.Date)
            .ToList();

        return new GetDailyNewUsersResult(
            new DailyNewUserStatsDto(normalizedPoints)
        );
    }
}
