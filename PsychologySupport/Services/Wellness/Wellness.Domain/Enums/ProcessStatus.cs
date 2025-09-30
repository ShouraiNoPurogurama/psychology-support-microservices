using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Wellness.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProcessStatus
    {
        NotStarted,
        Progressing,
        Completed,
        Skipped
    }
}
