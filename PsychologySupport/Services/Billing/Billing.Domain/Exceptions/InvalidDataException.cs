using Billing.Domain.Exceptions.Base;

namespace Billing.Domain.Exceptions
{
    public class InvalidDataException : BillingDomainException
    {
        public InvalidDataException() : base("Dữ liệu không hợp lệ.")
        {
        }
        public InvalidDataException(string message) : base(message)
        {
        }
    }
}
