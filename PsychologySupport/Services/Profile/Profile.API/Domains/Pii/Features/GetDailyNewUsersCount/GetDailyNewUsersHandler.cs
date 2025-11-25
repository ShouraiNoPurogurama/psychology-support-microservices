using Profile.API.Data.Pii;
using Profile.API.Domains.Pii.Dtos;

namespace Profile.API.Domains.Pii.Features.GetDailyNewUsersCount;

public record GetDailyNewUsersQuery(
    int? Year,
    int? Month
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
        var currentLocalDate = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7));
        
        var year = request.Year ?? currentLocalDate.Year;
        var month = request.Month ?? currentLocalDate.Month;
        
        var start = new DateOnly(year, month, 1);
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

        var dict = rawPoints.ToDictionary(x => x.Date, x => x.NewUserCount);

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
