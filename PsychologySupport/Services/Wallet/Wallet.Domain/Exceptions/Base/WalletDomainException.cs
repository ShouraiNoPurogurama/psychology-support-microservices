using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Domain.Exceptions.Base
{
    public class WalletDomainException : Exception
    {
        public WalletDomainException(string message) : base(message)
        {
        }
    }
}
