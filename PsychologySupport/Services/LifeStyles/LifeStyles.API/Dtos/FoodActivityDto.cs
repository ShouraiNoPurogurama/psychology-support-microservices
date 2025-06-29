namespace LifeStyles.API.Dtos;

public record FoodActivityDto(
    Guid Id,
    string Name,
    string Description,
    string MealTime,
    IEnumerable<string> FoodNutrients,
    IEnumerable<string> FoodCategories,
    string IntensityLevel
);