using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Domain.Exceptions
{
    public class WellnessDomainException : Exception
    {
        public WellnessDomainException(string message) : base(message)
        {
        }
    }
}
