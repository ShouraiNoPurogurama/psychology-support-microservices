using BuildingBlocks.Exceptions;

namespace Subscription.API.Exceptions;

public class SubscriptionNotFoundException : NotFoundException
{
    public SubscriptionNotFoundException(string? message) : base(message)
    {
    }

    public SubscriptionNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
    {
    }
}