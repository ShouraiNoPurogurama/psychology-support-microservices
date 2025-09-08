using BuildingBlocks.DDD;
using BuildingBlocks.Services;
using MediatR;

namespace BuildingBlocks.Behaviors;

public sealed class IdempotentCommandPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IdempotentCommand<TResponse>
    where TResponse : notnull
{
    private readonly IIdempotencyService _idempotencyService;

    public IdempotentCommandPipelineBehaviour(IIdempotencyService idempotencyService)
    {
        _idempotencyService = idempotencyService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var exists = await _idempotencyService.RequestExistsAsync(
            request.RequestKey,
            cancellationToken);

        if (exists)
        {
            return default!;
        }

        await _idempotencyService.CreateRequestAsync(
            request.RequestKey,
            cancellationToken);

        var response = await next();

        await _idempotencyService.SaveResponseAsync(
            request.RequestKey,
            response,
            cancellationToken);

        return response;
    }
}
