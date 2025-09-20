using Cassandra;
using Cassandra.Mapping;
using Feed.Application.Abstractions.UserActivity;
using Feed.Domain.UserActivity;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;
using Feed.Infrastructure.Persistence.Cassandra.Utils;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

public sealed class UserActivityRepository : IUserActivityRepository
{
    private readonly ISession _session;

    public UserActivityRepository(ISession session)
    {
        _session = session;
    }

    public async Task<bool> RecordSeenPostAsync(FeedSeenEntry seenEntry, CancellationToken ct)
    {
        var row = UserActivityMapper.ToRow(seenEntry);
        var cql = @"INSERT INTO feed_seen_by_day (AliasId, ymd, seen_at, post_id) VALUES (?, ?, ?, ?)";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(row.AliasId, row.Ymd, row.SeenAt, row.PostId).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<FeedSeenEntry>> GetSeenPostsByDayAsync(Guid aliasId, DateOnly ymd, CancellationToken ct)
    {
        var cassandraYmd = CassandraTypeMapper.ToLocalDate(ymd);
        var cql = @"SELECT AliasId, ymd, seen_at, post_id FROM feed_seen_by_day WHERE AliasId = ? AND ymd = ? ORDER BY seen_at DESC";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, cassandraYmd).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var list = new List<FeedSeenEntry>();
        foreach (var row in rs)
        {
            var seenRow = new FeedSeenByDayRow
            {
                AliasId = row.GetValue<Guid>("AliasId"),
                Ymd = row.GetValue<LocalDate>("ymd"),
                SeenAt = row.GetValue<TimeUuid>("seen_at"),
                PostId = row.GetValue<Guid>("post_id")
            };
            list.Add(UserActivityMapper.ToDomain(seenRow));
        }

        return list;
    }

    public async Task<IReadOnlyList<FeedSeenEntry>> GetRecentSeenPostsAsync(Guid aliasId, int days = 7, CancellationToken ct = default)
    {
        var allEntries = new List<FeedSeenEntry>();
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // Query each day separately due to Cassandra's partitioning
        for (int i = 0; i < days; i++)
        {
            var queryDate = currentDate.AddDays(-i);
            var dayEntries = await GetSeenPostsByDayAsync(aliasId, queryDate, ct);
            allEntries.AddRange(dayEntries);
        }

        return allEntries.OrderByDescending(e => e.SeenAt).ToList();
    }

    public async Task<bool> HasSeenPostTodayAsync(Guid aliasId, Guid postId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var cassandraToday = CassandraTypeMapper.ToLocalDate(today);
        var cql = @"SELECT AliasId FROM feed_seen_by_day WHERE AliasId = ? AND ymd = ? AND post_id = ? LIMIT 1 ALLOW FILTERING";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, cassandraToday, postId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return rs.Any();
    }
}
