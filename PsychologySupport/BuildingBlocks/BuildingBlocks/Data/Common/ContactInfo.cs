using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Data.Common
{
    public record ContactInfo(string? Address, string? PhoneNumber, string? Email)
    {
        public bool IsEnoughForUpdate()
        {
            return 
                    !string.IsNullOrWhiteSpace(Email)
                   && !string.IsNullOrWhiteSpace(Address);
        }
        
       public static ContactInfo Of(string? address, string? phoneNumber, string? email)
        {
            return new ContactInfo(address, phoneNumber, email);
        }
        
        public static ContactInfo Empty()
        {
            return new ContactInfo(null, null, null);
        }
    }
}
