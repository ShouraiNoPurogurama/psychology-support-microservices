using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Application.Features.Posts.Dtos;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Queries.GetPostCohort;

internal sealed class GetPostCohortsQueryHandler
    : IQueryHandler<GetPostCohortsQuery, GetPostCohortsResult>
{
    private readonly IPostDbContext _db;

    private static readonly TimeZoneInfo Tz = TimeZoneInfo.FindSystemTimeZoneById(
#if WINDOWS
        "SE Asia Standard Time"
#else
        "Asia/Ho_Chi_Minh"
#endif
    );

    public GetPostCohortsQueryHandler(IPostDbContext db) => _db = db;

    public async Task<GetPostCohortsResult> Handle(GetPostCohortsQuery q, CancellationToken ct)
    {
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Tz).Date);
        var start = q.StartDate;
        var endInclusive = todayLocal;

        // Giới hạn đọc từ DB theo PublishedAt (UTC) để giảm tải
        var startUtc = new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endUtcExclusive = new DateTimeOffset(endInclusive.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        // 1) Base events: user + ngày local (UTC+7)
        var baseEvents = await _db.Posts
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Visibility == PostVisibility.Public
                                     && p.PublishedAt >= startUtc && p.PublishedAt <= endUtcExclusive)
            .Select(p => new { p.Author.AliasId, p.PublishedAt })
            .ToListAsync(ct);

        // Map sang ngày local
        var baseUserDates = baseEvents
            .Select(e => new
            {
                UserId = e.AliasId,
                EventDate = DateOnly.FromDateTime(
                    TimeZoneInfo.ConvertTime(e.PublishedAt, Tz).Date
                )
            })
            .ToList();

        // 2) D0 mỗi user + lọc eligible
        var firstSeen = baseUserDates
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, D0 = g.Min(x => x.EventDate) })
            .Where(x => x.D0 >= start && x.D0 <= endInclusive)
            .ToList();

        if (firstSeen.Count == 0)
            return new GetPostCohortsResult(Array.Empty<PostCohortSeriesDto>());

        // 3) cohort_week = Monday-of-week (giống date_trunc('week'))
        static DateOnly MondayOfWeek(DateOnly d)
        {
            var dow = (int)d.DayOfWeek;
            // DayOfWeek: Sunday=0..Saturday=6 → muốn Monday=0
            var offset = dow == 0 ? 6 : dow - 1;
            return d.AddDays(-offset);
        }

        var eligible = firstSeen
            .Select(x => new { x.UserId, x.D0, CohortWeek = MondayOfWeek(x.D0) })
            .ToList();

        // 4) cohort sizes
        var cohortSizes = eligible
            .GroupBy(x => x.CohortWeek)
            .ToDictionary(g => g.Key, g => g.Count());

        // Index các ngày hoạt động của user để check nhanh
        var userEventDates = baseUserDates
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.EventDate).Distinct().OrderBy(d => d).ToArray()
            );

        // 5) Lưới offsets đủ (cắt theo hôm nay): với mỗi cohort_week, sinh Week k thỏa: cohort_week + k*7 <= today
        var maxK = q.MaxWeeks;
        var cohortWeeks = cohortSizes.Keys.OrderBy(d => d).ToArray();

        var grid = (from cw in cohortWeeks
                from k in Enumerable.Range(0, maxK + 1)
                let canShow = cw.AddDays(k * 7) <= endInclusive // hiển thị tới tuần hiện tại
                where canShow
                select new { CohortWeek = cw, WeekOffset = k })
            .ToList();

        // 6) Tính active: user có >=1 event trong [D0 + k*7, D0 + (k+1)*7)
        // Tối ưu đơn giản: duyệt user theo cohort, dùng mảng ngày đã index
        var activeCounts = new Dictionary<(DateOnly cw, int k), int>();

        var eligibleByCw = eligible.GroupBy(x => x.CohortWeek);
        foreach (var g in eligibleByCw)
        {
            var cw = g.Key;
            var users = g.ToArray();

            foreach (var k in Enumerable.Range(0, maxK + 1))
            {
                if (cw.AddDays(k * 7) > endInclusive) break;

                var count = 0;
                foreach (var u in users)
                {
                    if (!userEventDates.TryGetValue(u.UserId, out var dates)) continue;

                    var startK = u.D0.AddDays(k * 7);
                    var endKExclusive = u.D0.AddDays((k + 1) * 7);

                    // có ít nhất một ngày nằm trong [startK, endKExclusive)
                    // vì dates đã sort, check nhanh:
                    var any = dates.Any(d => d >= startK && d < endKExclusive);
                    if (any) count++;
                }

                activeCounts[(cw, k)] = count;
            }
        }

        // 7) Build kết quả đủ cột
        var series = cohortWeeks
            .Select(cw =>
            {
                cohortSizes.TryGetValue(cw, out var size);
                var points = Enumerable.Range(0, maxK + 1)
                    .Where(k => cw.AddDays(k * 7) <= endInclusive)
                    .Select(k =>
                    {
                        var active = activeCounts.TryGetValue((cw, k), out var c) ? c : 0;
                        var percent = size == 0 ? 0.0 : Math.Round(active * 100.0 / size, 1);
                        return new CohortPointDto(k, active, percent);
                    })
                    .ToList();

                return new PostCohortSeriesDto(cw, size, points);
            })
            .ToList();

        return new GetPostCohortsResult(series);
    }
}