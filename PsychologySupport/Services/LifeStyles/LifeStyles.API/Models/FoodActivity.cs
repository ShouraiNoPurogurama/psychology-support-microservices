using BuildingBlocks.Enums;

namespace LifeStyles.API.Models;

public class FoodActivity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MealTime MealTime { get; set; }
    public ICollection<FoodNutrient> FoodNutrients { get; set; } = [];
    public ICollection<FoodCategory> FoodCategories { get; set; } = [];
    public IntensityLevel IntensityLevel { get; set; }
}