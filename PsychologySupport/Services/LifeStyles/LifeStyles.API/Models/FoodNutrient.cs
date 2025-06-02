namespace LifeStyles.API.Models;

public class FoodNutrient
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<FoodActivity> FoodActivities { get; set; } = [];
}