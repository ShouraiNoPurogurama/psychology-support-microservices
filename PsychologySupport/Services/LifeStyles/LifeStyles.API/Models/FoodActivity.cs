using LifeStyles.API.Data.Common;

namespace LifeStyles.API.Models;

public class FoodActivity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public MealTime MealTime { get; set; }
    public ICollection<FoodNutrient> FoodNutrients { get; set; }
    public ICollection<FoodCategory> FoodCategories { get; set; }
    public IntensityLevel IntensityLevel { get; set; }
}