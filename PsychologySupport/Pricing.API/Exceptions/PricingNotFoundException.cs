using BuildingBlocks.Exceptions;

namespace Pricing.API.Exceptions;

public class PricingNotFoundException : NotFoundException
{
    public PricingNotFoundException(string? message) : base(message)
    {
    }

    public PricingNotFoundException(string entityName, Guid id)
        : base($"Entity \"{entityName}\" with Id {id} was not found.")
    {
    }
}
