using Microsoft.EntityFrameworkCore;
using Profile.API.Data.Pii;

namespace Profile.API.Domains.Pii.Features.GetTotalUsersCount;

public record GetTotalUsersCountQuery(
    int? Year,
    int? Month
) : IQuery<GetTotalUsersCountResult>;

public record GetTotalUsersCountResult(
    DateOnly Date,
    int TotalUsersCount
);

public class GetTotalUsersCountHandler(
    ILogger<GetTotalUsersCountHandler> logger,
    PiiDbContext dbContext
) : IQueryHandler<GetTotalUsersCountQuery, GetTotalUsersCountResult>
{
    public async Task<GetTotalUsersCountResult> Handle(
        GetTotalUsersCountQuery request,
        CancellationToken cancellationToken)
    {
        var nowLocal = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));

        if (request.Year is null && request.Month is null)
        {
            var totalAllTime = await dbContext.PersonProfiles
                .CountAsync(cancellationToken);

            var today = DateOnly.FromDateTime(nowLocal.Date);

            return new GetTotalUsersCountResult(
                today,
                totalAllTime
            );
        }

        var year = request.Year ?? nowLocal.Year;
        var month = request.Month ?? nowLocal.Month;

        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1);

        var startUtc = DateTime.SpecifyKind(
            start.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc
        );

        var endUtc = DateTime.SpecifyKind(
            end.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc
        );

        var startOffset = new DateTimeOffset(startUtc); 
        var endOffset = new DateTimeOffset(endUtc);   

        var totalUsersInMonth = await dbContext.PersonProfiles
            .Where(u => u.CreatedAt >= startOffset && u.CreatedAt < endOffset)
            .CountAsync(cancellationToken);

        var lastDayOfMonth = end.AddDays(-1);

        return new GetTotalUsersCountResult(
            lastDayOfMonth,
            totalUsersInMonth
        );
    }
}
