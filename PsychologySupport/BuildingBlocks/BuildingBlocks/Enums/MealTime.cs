using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MealTime
    {
        Breakfast,
        Lunch,
        Dinner,
        Snack,
        AllDay
    }
    
    public static class MealTimeExtensions
    {
        public static string ToReadableString(this MealTime mealTime)
        {
            return mealTime switch
            {
                MealTime.AllDay => "All Day",
                _ => mealTime.ToString()
            };
        }
    }
}