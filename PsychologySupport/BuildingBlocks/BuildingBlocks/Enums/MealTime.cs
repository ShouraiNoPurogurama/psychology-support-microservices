using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MealTime
    {
        Breakfast,
        Lunch,
        Dinner,
        Snack
    }
}