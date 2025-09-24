using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Wellness.Domain.Aggregates.Challenges.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActivityType
    {
        Meditation,
        Breathing,
        Journaling,
        Reading,
        Listening,
        Exercise,
        Other
    }
}
