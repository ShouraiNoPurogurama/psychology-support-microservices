// BuildingBlocks.Behaviors/IdempotentCommandPipelineBehaviour.cs

using BuildingBlocks.CQRS;
using BuildingBlocks.DDD;
using BuildingBlocks.Idempotency;

namespace BuildingBlocks.Behaviors;

public sealed class IdempotentCommandPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IdempotentCommand<TResponse>
    where TResponse : notnull
{
    private readonly IIdempotencyService _idemp;
    private readonly IIdempotencyHasher _hasher;
    private readonly IIdempotencyHashAccessor _hashAccessor;

    public IdempotentCommandPipelineBehaviour(
        IIdempotencyService idempotencyService,
        IIdempotencyHasher hasher,
        IIdempotencyHashAccessor hashAccessor)
    {
        _idemp = idempotencyService;
        _hasher = hasher;
        _hashAccessor = hashAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // 1) Tính & set hash từ chính command
        _hashAccessor.RequestHash = _hasher.ComputeHash(request);

        // 2) Nếu đã tồn tại → trả cached response (nếu có)
        var exists = await _idemp.RequestExistsAsync(request.RequestKey, ct);
        if (exists)
        {
            var (found, resp) = await _idemp.TryGetResponseAsync<TResponse>(request.RequestKey, ct);
            if (found && resp is not null) return resp;
            //nếu chưa có response (đang process ở replica khác), có thể: throw 409/429
        }

        // 3) Đăng ký request (DB + cache/lock do decorators xử lý)
        await _idemp.CreateRequestAsync(request.RequestKey, ct);

        var response = await next();

        await _idemp.SaveResponseAsync(request.RequestKey, response, ct);
        return response;
    }
}