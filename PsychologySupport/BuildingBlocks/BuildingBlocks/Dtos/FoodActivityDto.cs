using BuildingBlocks.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Dtos
{
    public record FoodActivityDto(
    Guid Id,
    string Name,
    string Description,
    MealTime MealTime,
    IEnumerable<string> FoodNutrients,
    IEnumerable<string> FoodCategories,
    IntensityLevel IntensityLevel
    ) : IActivityDto;
}
