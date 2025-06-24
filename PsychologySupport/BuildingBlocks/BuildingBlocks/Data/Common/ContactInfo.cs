﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
