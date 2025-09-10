using Billing.API.Exceptions.Base;

namespace Billing.API.Exceptions
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
