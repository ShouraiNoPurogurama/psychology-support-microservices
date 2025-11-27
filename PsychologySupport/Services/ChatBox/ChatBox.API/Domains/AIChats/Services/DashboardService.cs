using ChatBox.API.Data;
using ChatBox.API.Domains.AIChats.Dtos.Dashboard;
using ChatBox.API.Domains.AIChats.Services.Contracts;
using ChatBox.API.Models.Views;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Domains.AIChats.Services;

public class DashboardService(ChatBoxDbContext dbContext) : IDashboardService
{
    // Cross-platform timezone id
    private static readonly TimeZoneInfo Tz =
#if WINDOWS
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
#else
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
#endif

    // Nếu có fixed bot/system user thì để ở đây
    private static readonly Guid SystemUserId = Guid.Parse("0199e159-402d-7df5-8aa6-9baa9c461056");

    public async Task<ChatCohortResponseDto> GetChatCohortsAsync(
        DateOnly startDate,
        int maxWeeks,
        CancellationToken ct = default)
    {
        if (maxWeeks < 0) maxWeeks = 0;

        // Today theo giờ VN
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Tz).Date);

        // Giới hạn dữ liệu cần đọc (UTC)
        var startUtc = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endUtcInclusive = new DateTimeOffset(todayLocal.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        // 1) Base events (lọc ngay ở DB)
        var events = await dbContext.AIChatMessages
            .AsNoTracking()
            .Where(m => !m.SenderIsEmo
                        && m.SenderUserId != SystemUserId
                        && m.CreatedDate >= startUtc
                        && m.CreatedDate <= endUtcInclusive)
            .Select(m => new { m.SenderUserId, m.CreatedDate })
            .ToListAsync(ct);

        // 2) Map sang ngày local (UTC+7)
        var baseUserDates = events
            .Select(e => new
            {
                UserId = e.SenderUserId,
                EventDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(e.CreatedDate, Tz).Date)
            })
            .ToList();

        // 3) D0 mỗi user + filter eligible
        var firstSeen = baseUserDates
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, D0 = g.Min(x => x.EventDate) })
            .Where(x => x.D0 >= startDate && x.D0 <= todayLocal)
            .ToList();

        if (firstSeen.Count == 0)
            return new ChatCohortResponseDto([]);

        // 4) Monday-of-week (giống date_trunc('week'))
        static DateOnly MondayOfWeek(DateOnly d)
        {
            var dow = (int)d.DayOfWeek; // Sun=0 .. Sat=6
            var offset = dow == 0 ? 6 : dow - 1; // Monday=0
            return d.AddDays(-offset);
        }

        var eligible = firstSeen
            .Select(x => new { x.UserId, x.D0, CohortWeek = MondayOfWeek(x.D0) })
            .ToList();

        var cohortSizes = eligible
            .GroupBy(x => x.CohortWeek)
            .ToDictionary(g => g.Key, g => g.Count());

        // 5) Index ngày hoạt động theo user
        var userEventDates = baseUserDates
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.EventDate).Distinct().OrderBy(d => d).ToArray()
            );

        // 6) Sinh lưới offsets đầy đủ cho mỗi cohort (hiển thị đến tuần hiện tại)
        var cohortWeeks = cohortSizes.Keys.OrderBy(d => d).ToArray();
        var activeCounts = new Dictionary<(DateOnly cw, int k), int>();

        foreach (var cw in cohortWeeks)
        {
            var users = eligible.Where(e => e.CohortWeek == cw).ToArray();

            for (int k = 0; k <= maxWeeks; k++)
            {
                // Chỉ show nếu đã tới tuần này (bao gồm tuần đang chạy)
                if (cw.AddDays(k * 7) > todayLocal) break;

                int active = 0;

                foreach (var u in users)
                {
                    if (!userEventDates.TryGetValue(u.UserId, out var days)) continue;

                    var startK = u.D0.AddDays(k * 7);
                    var endKExclusive = u.D0.AddDays((k + 1) * 7);

                    // có >=1 activity trong [startK, endK)
                    // (days đã sort nên Any đơn giản là đủ, nếu cần tối ưu: binary search)
                    if (days.Any(d => d >= startK && d < endKExclusive))
                        active++;
                }

                activeCounts[(cw, k)] = active;
            }
        }

        // 7) Build DTO
        var series = cohortWeeks
            .Select(cw =>
            {
                var size = cohortSizes[cw];
                var points = new List<ChatCohortPointDto>();

                for (int k = 0; k <= maxWeeks; k++)
                {
                    if (cw.AddDays(k * 7) > todayLocal) break;

                    var active = activeCounts.TryGetValue((cw, k), out var c) ? c : 0;
                    var percent = size == 0 ? 0.0 : Math.Round(active * 100.0 / size, 1);

                    points.Add(new ChatCohortPointDto(k, active, percent));
                }

                return new ChatCohortSeriesDto(cw, size, points);
            })
            .ToList();

        return new ChatCohortResponseDto(series);
    }

    public async Task<UserOnscreenStatsDto> GetUsersChatOnscreenStatsAsync(
        DateOnly startDate,
        int maxWeeks = 7,
        CancellationToken ct = default)
    {
        if (maxWeeks < 0) maxWeeks = 0;

        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Tz).Date);

        var startUtc = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endUtcInclusive = new DateTimeOffset(todayLocal.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var userOnscreenPointDtos = await dbContext.UserOnScreenStats
            .Where(s => s.ActivityDate >= startUtc && s.ActivityDate <= endUtcInclusive)
            .OrderByDescending(s => s.ActivityDate)
            .ProjectToType<UserOnscreenPointDto>()
            .ToListAsync(cancellationToken: ct);

        var allTimeStats = userOnscreenPointDtos
            .GroupBy(x => 1) // Trick: Group tất cả vào 1 key (số 1)
            .Select(g => new
            {
                TotalSystemOnscreenSeconds = g.Sum(x => x.TotalSystemOnscreenSeconds),
                AvgOnscreenSecondsPerUser = g.Average(x => x.AvgOnscreenSecondsPerUser),
                TotalActiveUsers = g.Sum(x => x.TotalActiveUsers)
            })
            .First();

        var usersOnscreenStats = new UserOnscreenStatsDto(userOnscreenPointDtos, allTimeStats.TotalActiveUsers,
            allTimeStats.TotalSystemOnscreenSeconds, allTimeStats.AvgOnscreenSecondsPerUser);

        return usersOnscreenStats;
    }

    public async Task<DailyUserRetentionReportDto> GetRetentionReportAsync(
        DateOnly startDate,
        int maxWeeks = 7,
        CancellationToken ct = default)
    {
        if (maxWeeks < 0) maxWeeks = 0;

        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Tz).Date);

        var startUtc = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endUtcInclusive = new DateTimeOffset(todayLocal.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var retentionPoints = await dbContext.DailyUserRetentionStats
            .AsNoTracking()
            .Where(s => s.ActivityDate >= startUtc && s.ActivityDate <= endUtcInclusive)
            .OrderByDescending(s => s.ActivityDate)
            .ProjectToType<DailyUserRetentionPointDto>() // Mapster
            .ToListAsync(cancellationToken: ct);

        decimal currentTotalUsers = 0;
        decimal avgRetention = 0;

        if (retentionPoints.Count > 0)
        {
            currentTotalUsers = retentionPoints.Max(x => x.TotalUsersToDate);

            avgRetention = retentionPoints.Average(x => x.ReturningPercentage);
        }

        return new DailyUserRetentionReportDto(
            retentionPoints,
            currentTotalUsers,
            Math.Round(avgRetention, 2)
        );
    }
}