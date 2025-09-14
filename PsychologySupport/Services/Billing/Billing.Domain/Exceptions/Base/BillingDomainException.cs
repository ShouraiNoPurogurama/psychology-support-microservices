using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Billing.Domain.Exceptions.Base
{
    public class BillingDomainException : Exception
    {
        public BillingDomainException(string message) : base(message)
        {
        }
    }
}
