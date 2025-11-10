using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Wellness.Domain.Enums
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TagScope
    {
        Free = 0,   
        Paid = 1   
    }
}
