using LifeStyles.API.Data.Common;

namespace LifeStyles.API.Dtos;

public record FoodActivityDto(
    Guid Id,
    string Name,
    string Description,
    MealTime MealTime,
    IEnumerable<string> FoodNutrients,
    IEnumerable<string> FoodCategories,
    IntensityLevel IntensityLevel
);