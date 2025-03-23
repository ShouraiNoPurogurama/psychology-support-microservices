using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IntensityLevel
    {
        Low,
        Medium,
        High,
        VeryHigh
    }
    
    public static class IntensityLevelExtensions
    {
        public static string ToReadableString(this IntensityLevel level)
        {
            return level switch
            {
                IntensityLevel.VeryHigh => "Very High",
                _ => level.ToString()
            };
        }
    }
}