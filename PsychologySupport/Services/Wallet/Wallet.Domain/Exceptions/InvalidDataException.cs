using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Exceptions.Base;

namespace Wallet.Domain.Exceptions
{
    public class InvalidDataException : WalletDomainException
    {
        public InvalidDataException() : base("Dữ liệu không hợp lệ.")
        {
        }
        public InvalidDataException(string message) : base(message)
        {
        }
    }
}
