using BuildingBlocks.CQRS;
using MediatR;

namespace BuildingBlocks.DDD;

public abstract record IdempotentCommand<TResponse>(Guid RequestKey)
        : ICommand<TResponse> where TResponse : notnull;
    