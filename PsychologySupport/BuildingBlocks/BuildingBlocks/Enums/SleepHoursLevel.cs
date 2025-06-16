using System;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SleepHoursLevel
    {
        LessThan4Hours,
        From4To6Hours,
        From6To8Hours,
        From8To10Hours,
        MoreThan10Hours
    }

    public static class SleepHoursLevelExtensions
    {
        public static string ToReadableString(this SleepHoursLevel level)
        {
            return level switch
            {
                SleepHoursLevel.LessThan4Hours => "Less 4 hours",
                SleepHoursLevel.From4To6Hours => "4–6 hours",
                SleepHoursLevel.From6To8Hours => "6–8 hours",
                SleepHoursLevel.From8To10Hours => "8–10 hours",
                SleepHoursLevel.MoreThan10Hours => "More 10 hours",
                _ => level.ToString()
            };
        }
    }
}
