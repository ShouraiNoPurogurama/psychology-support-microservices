using BuildingBlocks.Exceptions;

namespace Billing.API.Exceptions
{
    public class BillingNotFoundException : NotFoundException
    {
        public BillingNotFoundException(string? message) : base(message)
        {
        }

        public BillingNotFoundException(string name, Guid id) : base($"Entity \"{name}\" with Id {id} was not found.")
        {
        }
    }
}
