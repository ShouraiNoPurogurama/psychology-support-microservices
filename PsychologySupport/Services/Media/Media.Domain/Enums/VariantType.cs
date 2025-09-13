using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Media.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VariantType
    {
        Thumbnail,
        Medium,
        Original
    }
}
