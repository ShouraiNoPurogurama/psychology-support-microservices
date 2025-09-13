namespace BuildingBlocks.CQRS;

public abstract record IdempotentCommand<TResponse>(Guid RequestKey)
    : ICommand<TResponse> 
    where TResponse : notnull;