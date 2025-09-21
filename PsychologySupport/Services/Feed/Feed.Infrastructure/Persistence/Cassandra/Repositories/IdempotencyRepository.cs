using Cassandra;
using Feed.Domain.IdempotencyKey;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

using Cassandra;
using Feed.Application.Abstractions.Resilience;

public sealed class IdempotencyRepository : IIdempotencyRepository
{
    private readonly ISession _session;
    private readonly IPreparedStatementRegistry _statements;

    // Prepared statement keys
    private const string GET_REQUEST = nameof(GET_REQUEST);
    private const string CREATE_REQUEST = nameof(CREATE_REQUEST);
    private const string UPDATE_REQUEST = nameof(UPDATE_REQUEST);

    // CQL Statements
    private const string GET_CQL = @"SELECT key, request_hash, response_payload, created_at, created_by_alias_id, expires_at 
                                    FROM idempotency_keys WHERE key = ?";

    // Using 'IF NOT EXISTS' makes this an atomic, lightweight transaction (LWT).
    // Using 'USING TTL' lets Cassandra handle automatic key expiration.
    private const string CREATE_CQL =
        @"INSERT INTO idempotency_keys (key, request_hash, created_at, created_by_alias_id, expires_at) 
                                       VALUES (?, ?, ?, ?, ?) IF NOT EXISTS USING TTL ?";

    private const string UPDATE_CQL = @"UPDATE idempotency_keys USING TTL ? SET response_payload = ?, expires_at = ? 
                                       WHERE key = ?";

    public IdempotencyRepository(ISession session, IPreparedStatementRegistry statements)
    {
        _session = session;
        _statements = statements;
    }

    public async Task<IdempotencyKey?> GetAsync(Guid key, CancellationToken ct = default)
    {
        var ps = await _statements.GetStatementAsync(GET_REQUEST, GET_CQL);
        var stmt = ps.Bind(key)
            .SetConsistencyLevel(ConsistencyLevel.LocalQuorum) // Stronger consistency for idempotency checks
            .SetIdempotence(true);

        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        var row = rs.FirstOrDefault();

        var idempotencyKeyRow = row is not null
            ? new IdempotencyKeyRow
            {
                Key = row.GetValue<Guid>("key"),
                RequestHash = row.GetValue<string>("request_hash"),
                ResponsePayload = row.IsNull("response_payload") ? null : row.GetValue<string>("response_payload"),
                CreatedAt = row.IsNull("created_at") ? (DateTimeOffset?)null : row.GetValue<DateTimeOffset>("created_at"),
                CreatedByAliasId = row.GetValue<Guid>("created_by_alias_id"),
                ExpiresAt = row.IsNull("expires_at") ? (DateTimeOffset?)null : row.GetValue<DateTimeOffset>("expires_at")
            }
            : null;

        return idempotencyKeyRow is not null ? IdempotencyKeyMapper.ToDomain(idempotencyKeyRow) : null;
    }

    public async Task<bool> TryCreateAsync(IdempotencyKey request, TimeSpan ttl, CancellationToken ct = default)
    {
        var row = IdempotencyKeyMapper.ToRow(request);
        var ps = await _statements.GetStatementAsync(CREATE_REQUEST, CREATE_CQL);

        var stmt = ps.Bind(
                row.Key,
                row.RequestHash,
                row.CreatedAt,
                row.CreatedByAliasId,
                row.ExpiresAt,
                (int)ttl.TotalSeconds)
            .SetConsistencyLevel(ConsistencyLevel.Quorum)
            .SetIdempotence(true);

        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        // For an LWT, the first column of the first row is '[applied]'
        return rs.FirstOrDefault()?.GetValue<bool>("[applied]") ?? false;
    }

    public async Task UpdateAsync(IdempotencyKey request, TimeSpan ttl, CancellationToken ct = default)
    {
        var row = IdempotencyKeyMapper.ToRow(request);
        var ps = await _statements.GetStatementAsync(UPDATE_REQUEST, UPDATE_CQL);

        var stmt = ps.Bind(
                (int)ttl.TotalSeconds,
                row.ResponsePayload,
                row.ExpiresAt,
                row.Key)
            .SetConsistencyLevel(ConsistencyLevel.Quorum)
            .SetIdempotence(true);

        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
    }
}