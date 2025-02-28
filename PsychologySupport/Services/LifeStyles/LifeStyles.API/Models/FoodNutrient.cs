namespace LifeStyles.API.Models;

public class FoodNutrient
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<FoodActivity> FoodActivities { get; set; }
}