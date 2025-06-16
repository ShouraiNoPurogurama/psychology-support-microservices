using System;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AvailableTimePerDay
    {
        LessThan30Minutes,
        From30To60Minutes,
        MoreThan1Hour
    }

    public static class AvailableTimePerDayExtensions
    {
        public static string ToReadableString(this AvailableTimePerDay time)
        {
            return time switch
            {
                AvailableTimePerDay.LessThan30Minutes => "Less 30 minutes",
                AvailableTimePerDay.From30To60Minutes => "30–60 minutes",
                AvailableTimePerDay.MoreThan1Hour => "More 1 hour",
                _ => time.ToString()
            };
        }
    }
}
