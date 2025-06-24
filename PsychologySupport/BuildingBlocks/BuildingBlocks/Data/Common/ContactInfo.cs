using System;

namespace BuildingBlocks.Data.Common
{
    public record ContactInfo(string Address, string PhoneNumber, string Email)
    {
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(PhoneNumber)
                   && !string.IsNullOrWhiteSpace(Email)
                   && !string.IsNullOrWhiteSpace(Address);
        }
    }
}
