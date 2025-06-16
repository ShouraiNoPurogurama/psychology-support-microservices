using System;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExerciseFrequency
    {
        Never,
        Rarely,
        TwoToThreePerWeek,
        Daily
    }

    public static class ExerciseFrequencyExtensions
    {
        public static string ToReadableString(this ExerciseFrequency frequency)
        {
            return frequency switch
            {
                ExerciseFrequency.TwoToThreePerWeek => "2–3 times per week",
                _ => frequency.ToString()
            };
        }
    }
}
