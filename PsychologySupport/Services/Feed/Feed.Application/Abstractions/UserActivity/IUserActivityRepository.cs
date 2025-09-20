using Cassandra;
using Feed.Domain.UserActivity;

namespace Feed.Application.Abstractions.UserActivity;

public interface IUserActivityRepository
{
    Task<bool> RecordSeenPostAsync(FeedSeenEntry seenEntry, CancellationToken ct);
    Task<IReadOnlyList<FeedSeenEntry>> GetSeenPostsByDayAsync(Guid aliasId, LocalDate ymd, CancellationToken ct);
    Task<IReadOnlyList<FeedSeenEntry>> GetRecentSeenPostsAsync(Guid aliasId, int days = 7, CancellationToken ct);
    Task<bool> HasSeenPostTodayAsync(Guid aliasId, Guid postId, CancellationToken ct);
}
