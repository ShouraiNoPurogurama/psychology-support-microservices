using Cassandra;
using Feed.Application.Abstractions.PostEngagement;
using Feed.Domain.PostReplica;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;
using Microsoft.Extensions.Logging;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

/// <summary>
/// Cassandra repository for post engagement counters operations.
/// Handles counter increments and reads for engagement metrics.
/// </summary>
public sealed class PostEngagementCountersRepository : IPostEngagementCountersRepository
{
    private readonly ISession _session;
    private readonly IPreparedStatementRegistry _statements;
    private readonly ILogger<PostEngagementCountersRepository> _logger;

    // Prepared statement keys
    private const string INCREMENT_REACTIONS = nameof(INCREMENT_REACTIONS);
    private const string INCREMENT_COMMENTS = nameof(INCREMENT_COMMENTS);
    private const string INCREMENT_SHARES = nameof(INCREMENT_SHARES);
    private const string INCREMENT_CLICKS = nameof(INCREMENT_CLICKS);
    private const string INCREMENT_IMPRESSIONS = nameof(INCREMENT_IMPRESSIONS);
    private const string INCREMENT_VIEW_DURATION = nameof(INCREMENT_VIEW_DURATION);
    private const string SELECT_COUNTERS = nameof(SELECT_COUNTERS);
    private const string DELETE_COUNTERS = nameof(DELETE_COUNTERS);

    // CQL statements - Counter updates
    private const string INCREMENT_REACTIONS_CQL = @"
        UPDATE PostEngagementCounters
        SET reactions = reactions + ?
        WHERE post_id = ?";

    private const string INCREMENT_COMMENTS_CQL = @"
        UPDATE PostEngagementCounters
        SET comments = comments + ?
        WHERE post_id = ?";

    private const string INCREMENT_SHARES_CQL = @"
        UPDATE PostEngagementCounters
        SET shares = shares + ?
        WHERE post_id = ?";

    private const string INCREMENT_CLICKS_CQL = @"
        UPDATE PostEngagementCounters
        SET clicks = clicks + ?
        WHERE post_id = ?";

    private const string INCREMENT_IMPRESSIONS_CQL = @"
        UPDATE PostEngagementCounters
        SET impressions = impressions + ?
        WHERE post_id = ?";

    private const string INCREMENT_VIEW_DURATION_CQL = @"
        UPDATE PostEngagementCounters
        SET view_duration_sec = view_duration_sec + ?
        WHERE post_id = ?";

    private const string SELECT_COUNTERS_CQL = @"
        SELECT post_id, reactions, comments, shares, clicks, impressions, view_duration_sec
        FROM PostEngagementCounters
        WHERE post_id = ?";

    private const string DELETE_COUNTERS_CQL = @"
        DELETE FROM PostEngagementCounters WHERE post_id = ?";

    public PostEngagementCountersRepository(
        ISession session,
        IPreparedStatementRegistry statements,
        ILogger<PostEngagementCountersRepository> logger)
    {
        _session = session;
        _statements = statements;
        _logger = logger;
    }

    public async Task<bool> IncrementReactionsAsync(
        Guid postId,
        long increment = 1,
        CancellationToken ct = default)
    {
        return await IncrementCounterAsync(postId, increment, INCREMENT_REACTIONS, INCREMENT_REACTIONS_CQL, "reactions");
    }

    public async Task<bool> IncrementCommentsAsync(
        Guid postId,
        long increment = 1,
        CancellationToken ct = default)
    {
        return await IncrementCounterAsync(postId, increment, INCREMENT_COMMENTS, INCREMENT_COMMENTS_CQL, "comments");
    }

    public async Task<bool> IncrementSharesAsync(
        Guid postId,
        long increment = 1,
        CancellationToken ct = default)
    {
        return await IncrementCounterAsync(postId, increment, INCREMENT_SHARES, INCREMENT_SHARES_CQL, "shares");
    }

    public async Task<bool> IncrementClicksAsync(
        Guid postId,
        long increment = 1,
        CancellationToken ct = default)
    {
        return await IncrementCounterAsync(postId, increment, INCREMENT_CLICKS, INCREMENT_CLICKS_CQL, "clicks");
    }

    public async Task<bool> IncrementImpressionsAsync(
        Guid postId,
        long increment = 1,
        CancellationToken ct = default)
    {
        return await IncrementCounterAsync(postId, increment, INCREMENT_IMPRESSIONS, INCREMENT_IMPRESSIONS_CQL, "impressions");
    }

    public async Task<bool> IncrementViewDurationAsync(
        Guid postId,
        long seconds,
        CancellationToken ct = default)
    {
        return await IncrementCounterAsync(postId, seconds, INCREMENT_VIEW_DURATION, INCREMENT_VIEW_DURATION_CQL, "view_duration");
    }

    public async Task<PostEngagementCounters?> GetCountersAsync(
        Guid postId,
        CancellationToken ct = default)
    {
        try
        {
            var ps = await _statements.GetStatementAsync(SELECT_COUNTERS, SELECT_COUNTERS_CQL);
            var stmt = ps.Bind(postId)
                .SetConsistencyLevel(ConsistencyLevel.LocalOne)
                .SetIdempotence(true);

            var rs = await _session.ExecuteAsync(stmt);
            var row = rs.FirstOrDefault();

            if (row == null)
                return null;

            var countersRow = new PostEngagementCountersRow
            {
                PostId = row.GetValue<Guid>("post_id"),
                Reactions = row.GetValue<long?>("reactions"),
                Comments = row.GetValue<long?>("comments"),
                Shares = row.GetValue<long?>("shares"),
                Clicks = row.GetValue<long?>("clicks"),
                Impressions = row.GetValue<long?>("impressions"),
                ViewDurationSec = row.GetValue<long?>("view_duration_sec")
            };

            return PostEngagementMapper.ToDomain(countersRow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting engagement counters for post {PostId}", postId);
            throw;
        }
    }

    public async Task<IReadOnlyDictionary<Guid, PostEngagementCounters>> GetCountersBatchAsync(
        IEnumerable<Guid> postIds,
        CancellationToken ct = default)
    {
        try
        {
            var result = new Dictionary<Guid, PostEngagementCounters>();
            var ps = await _statements.GetStatementAsync(SELECT_COUNTERS, SELECT_COUNTERS_CQL);

            // Execute queries in parallel for better performance
            var tasks = postIds.Select(async postId =>
            {
                var stmt = ps.Bind(postId)
                    .SetConsistencyLevel(ConsistencyLevel.LocalOne)
                    .SetIdempotence(true);

                var rs = await _session.ExecuteAsync(stmt);
                var row = rs.FirstOrDefault();

                if (row != null)
                {
                    var countersRow = new PostEngagementCountersRow
                    {
                        PostId = row.GetValue<Guid>("post_id"),
                        Reactions = row.GetValue<long?>("reactions"),
                        Comments = row.GetValue<long?>("comments"),
                        Shares = row.GetValue<long?>("shares"),
                        Clicks = row.GetValue<long?>("clicks"),
                        Impressions = row.GetValue<long?>("impressions"),
                        ViewDurationSec = row.GetValue<long?>("view_duration_sec")
                    };

                    return (postId, counters: PostEngagementMapper.ToDomain(countersRow));
                }

                return (postId, counters: (PostEngagementCounters?)null);
            });

            var results = await Task.WhenAll(tasks);

            foreach (var (postId, counters) in results)
            {
                if (counters != null)
                {
                    result[postId] = counters;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting engagement counters for multiple posts");
            throw;
        }
    }

    public async Task<bool> DeleteCountersAsync(
        Guid postId,
        CancellationToken ct = default)
    {
        try
        {
            var ps = await _statements.GetStatementAsync(DELETE_COUNTERS, DELETE_COUNTERS_CQL);
            var stmt = ps.Bind(postId)
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetIdempotence(true);

            await _session.ExecuteAsync(stmt);

            _logger.LogDebug("Deleted engagement counters for post {PostId}", postId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting engagement counters for post {PostId}", postId);
            throw;
        }
    }

    // Helper method to reduce code duplication
    private async Task<bool> IncrementCounterAsync(
        Guid postId,
        long increment,
        string statementKey,
        string cql,
        string counterName)
    {
        try
        {
            var ps = await _statements.GetStatementAsync(statementKey, cql);
            var stmt = ps.Bind(increment, postId)
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetIdempotence(true);

            await _session.ExecuteAsync(stmt);

            _logger.LogDebug("Incremented {CounterName} by {Increment} for post {PostId}", counterName, increment, postId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing {CounterName} for post {PostId}", counterName, postId);
            throw;
        }
    }
}
