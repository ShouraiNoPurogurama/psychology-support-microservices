using System.Text.Json.Serialization;

namespace LifeStyles.API.Data.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MealTime
{
    Breakfast,
    Lunch,
    Dinner,
    Snack
}