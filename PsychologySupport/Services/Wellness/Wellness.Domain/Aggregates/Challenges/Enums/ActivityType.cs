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
        Mental,    // Meditation, Breathing, Journaling, Gratitude, Mindfulness, Reading, Listening
        Physical,  // Exercise, Walking, Stretching, Relaxation, Nature
        Social,    // Social connection, talking with friends, group activities
        Creative,  // Art, music, writing, crafting
        Other      // Anything not covered above
    }

}
