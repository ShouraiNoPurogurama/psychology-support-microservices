namespace Billing.API.Exceptions.Base
{
    public class BillingDomainException : Exception
    {
        public BillingDomainException(string message) : base(message)
        {
        }
    }
}
