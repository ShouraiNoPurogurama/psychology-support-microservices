using System.Text.Json;
using BuildingBlocks.Idempotency;
using Feed.Application.Abstractions.Resilience;
using Feed.Domain.IdempotencyKey;

namespace Feed.Infrastructure.Resilience.Services;

public sealed class CassandraIdempotencyService : IIdempotencyService
{
    private readonly IIdempotencyRepository _repository;
    private readonly IIdempotencyHashAccessor _hashAccessor;
    private readonly TimeSpan _ttl = TimeSpan.FromHours(12); // Default TTL

    public CassandraIdempotencyService(IIdempotencyRepository repository, IIdempotencyHashAccessor hashAccessor)
    {
        _repository = repository;
        _hashAccessor = hashAccessor;
    }

    public async Task<bool> RequestExistsAsync(Guid requestKey, CancellationToken ct = default)
    {
        var request = await _repository.GetAsync(requestKey, ct);
        return request is not null;
    }

    public async Task<(bool Found, T? Response)> TryGetResponseAsync<T>(Guid requestKey, CancellationToken ct = default)
    {
        var request = await _repository.GetAsync(requestKey, ct);

        if (request?.ResponsePayload is null)
        {
            return (false, default);
        }

        var response = JsonSerializer.Deserialize<T>(request.ResponsePayload);
        return (true, response);
    }

    // Lưu ý: Phương thức này không còn trả về ID nội bộ nữa, nó trả về chính requestKey.
    // Điều này không ảnh hưởng vì pipeline behavior không thực sự dùng ID đó.
    public async Task<Guid> CreateRequestAsync(Guid requestKey, CancellationToken ct = default)
    {
        var requestHash = _hashAccessor.RequestHash ?? throw new InvalidOperationException("Request hash is not available.");
        // TODO: Get current user's AliasId from a user context service
        var createdByAliasId = Guid.NewGuid(); // Placeholder

        var newRequest = IdempotencyKey.Create(
            requestKey,
            requestHash,
            createdByAliasId,
            DateTimeOffset.UtcNow.Add(_ttl)
        );

        var created = await _repository.TryCreateAsync(newRequest, _ttl, ct);

        if (!created)
        {
            // If creation failed, it means the key exists. We must check the hash for safety.
            var existingRequest = await _repository.GetAsync(requestKey, ct);
            if (existingRequest is not null && !string.Equals(existingRequest.RequestHash, requestHash, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Idempotency-Key reused with different request payload.");
            }
        }
        
        return requestKey;
    }

    public async Task SaveResponseAsync<T>(Guid requestKey, T response, CancellationToken ct = default)
    {
        var existingRequest = await _repository.GetAsync(requestKey, ct)
                              ?? throw new KeyNotFoundException($"Idempotency key {requestKey} not found.");

        var payload = JsonSerializer.Serialize(response);
        existingRequest.SetResponse(payload);

        await _repository.UpdateAsync(existingRequest, _ttl, ct);
    }
}