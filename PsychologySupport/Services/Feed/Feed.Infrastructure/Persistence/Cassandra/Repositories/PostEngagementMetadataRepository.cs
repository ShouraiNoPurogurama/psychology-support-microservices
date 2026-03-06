using Cassandra;
using Feed.Application.Abstractions.PostEngagement;
using Feed.Domain.PostReplica;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;
using Microsoft.Extensions.Logging;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

/// <summary>
/// Cassandra repository for post engagement metadata operations.
/// </summary>
public sealed class PostEngagementMetadataRepository : IPostEngagementMetadataRepository
{
    private readonly ISession _session;
    private readonly IPreparedStatementRegistry _statements;
    private readonly ILogger<PostEngagementMetadataRepository> _logger;

    // Prepared statement keys
    private const string INSERT_METADATA = nameof(INSERT_METADATA);
    private const string SELECT_METADATA = nameof(SELECT_METADATA);
    private const string UPDATE_COUNTERS_TIMESTAMP = nameof(UPDATE_COUNTERS_TIMESTAMP);
    private const string DELETE_METADATA = nameof(DELETE_METADATA);
    private const string EXISTS_METADATA = nameof(EXISTS_METADATA);

    // CQL statements
    private const string INSERT_METADATA_CQL = @"
        INSERT INTO PostEngagementMetadata (post_id, author_alias_id, created_at, counters_last_updated)
        VALUES (?, ?, ?, ?)";

    private const string SELECT_METADATA_CQL = @"
        SELECT post_id, author_alias_id, created_at, counters_last_updated
        FROM PostEngagementMetadata
        WHERE post_id = ?";

    private const string UPDATE_COUNTERS_TIMESTAMP_CQL = @"
        UPDATE PostEngagementMetadata
        SET counters_last_updated = ?
        WHERE post_id = ?";

    private const string DELETE_METADATA_CQL = @"
        DELETE FROM PostEngagementMetadata WHERE post_id = ?";

    private const string EXISTS_METADATA_CQL = @"
        SELECT post_id FROM PostEngagementMetadata WHERE post_id = ? LIMIT 1";

    public PostEngagementMetadataRepository(
        ISession session,
        IPreparedStatementRegistry statements,
        ILogger<PostEngagementMetadataRepository> logger)
    {
        _session = session;
        _statements = statements;
        _logger = logger;
    }

    public async Task<bool> UpsertMetadataAsync(
        PostEngagementMetadata metadata,
        CancellationToken ct = default)
    {
        try
        {
            var row = PostEngagementMapper.ToRow(metadata);
            var ps = await _statements.GetStatementAsync(INSERT_METADATA, INSERT_METADATA_CQL);
            
            var stmt = ps.Bind(
                row.PostId,
                row.AuthorAliasId,
                row.CreatedAt,
                row.CountersLastUpdated)
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetIdempotence(true);

            await _session.ExecuteAsync(stmt);

            _logger.LogDebug("Upserted engagement metadata for post {PostId}", metadata.PostId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting engagement metadata for post {PostId}", metadata.PostId);
            throw;
        }
    }

    public async Task<PostEngagementMetadata?> GetMetadataAsync(
        Guid postId,
        CancellationToken ct = default)
    {
        try
        {
            var ps = await _statements.GetStatementAsync(SELECT_METADATA, SELECT_METADATA_CQL);
            var stmt = ps.Bind(postId)
                .SetConsistencyLevel(ConsistencyLevel.LocalOne)
                .SetIdempotence(true);

            var rs = await _session.ExecuteAsync(stmt);
            var row = rs.FirstOrDefault();

            if (row == null)
                return null;

            var metadataRow = new PostEngagementMetadataRow
            {
                PostId = row.GetValue<Guid>("post_id"),
                AuthorAliasId = row.GetValue<Guid>("author_alias_id"),
                CreatedAt = row.GetValue<DateTimeOffset>("created_at"),
                CountersLastUpdated = row.GetValue<DateTimeOffset>("counters_last_updated")
            };

            return PostEngagementMapper.ToDomain(metadataRow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting engagement metadata for post {PostId}", postId);
            throw;
        }
    }

    public async Task<bool> UpdateCountersTimestampAsync(
        Guid postId,
        DateTimeOffset timestamp,
        CancellationToken ct = default)
    {
        try
        {
            var ps = await _statements.GetStatementAsync(UPDATE_COUNTERS_TIMESTAMP, UPDATE_COUNTERS_TIMESTAMP_CQL);
            var stmt = ps.Bind(timestamp, postId)
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetIdempotence(true);

            await _session.ExecuteAsync(stmt);

            _logger.LogDebug("Updated counters timestamp for post {PostId}", postId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating counters timestamp for post {PostId}", postId);
            throw;
        }
    }

    public async Task<bool> DeleteMetadataAsync(
        Guid postId,
        CancellationToken ct = default)
    {
        try
        {
            var ps = await _statements.GetStatementAsync(DELETE_METADATA, DELETE_METADATA_CQL);
            var stmt = ps.Bind(postId)
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetIdempotence(true);

            await _session.ExecuteAsync(stmt);

            _logger.LogDebug("Deleted engagement metadata for post {PostId}", postId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting engagement metadata for post {PostId}", postId);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(
        Guid postId,
        CancellationToken ct = default)
    {
        try
        {
            var ps = await _statements.GetStatementAsync(EXISTS_METADATA, EXISTS_METADATA_CQL);
            var stmt = ps.Bind(postId)
                .SetConsistencyLevel(ConsistencyLevel.LocalOne)
                .SetIdempotence(true);

            var rs = await _session.ExecuteAsync(stmt);
            return rs.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of engagement metadata for post {PostId}", postId);
            throw;
        }
    }
}
