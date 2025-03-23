using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ImpactLevel
    {
        Low,
        Medium,
        High,
        VeryHigh
    }

    public static class ImpactLevelExtensions
    {
        public static string ToReadableString(this ImpactLevel impactLevel)
        {
            return impactLevel switch
            {
                ImpactLevel.VeryHigh => "Very High",
                _ => impactLevel.ToString()
            };
        }
    }
}