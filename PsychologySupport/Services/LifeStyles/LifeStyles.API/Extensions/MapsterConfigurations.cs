using System.Reflection;
using LifeStyles.API.Dtos;
using LifeStyles.API.Models;
using Mapster;

namespace LifeStyles.API.Extensions;

public static class MapsterConfigurations
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());


        TypeAdapterConfig<FoodActivity, FoodActivityDto>
        .NewConfig()
        .Map(dest => dest.Id, src => src.Id)
        .Map(dest => dest.Name, src => src.Name)
        .Map(dest => dest.Description, src => src.Description)
        .Map(dest => dest.MealTime, src => src.MealTime) 
        .Map(dest => dest.IntensityLevel, src => src.IntensityLevel) 
        .Map(dest => dest.FoodNutrients, src => src.FoodNutrients.Select(fn => fn.Name)) 
        .Map(dest => dest.FoodCategories, src => src.FoodCategories.Select(fc => fc.Name)); 


        TypeAdapterConfig<EntertainmentActivity, EntertainmentActivityDto>
          .NewConfig();

        TypeAdapterConfig<PhysicalActivity, PhysicalActivityDto>
          .NewConfig();

        TypeAdapterConfig<TherapeuticActivity, TherapeuticActivityDto>
          .NewConfig();

        TypeAdapterConfig<LifestyleLog, LifestyleLogDto>
          .NewConfig();

        TypeAdapterConfig<ImprovementGoal, ImprovementGoalDto>
            .NewConfig();

        TypeAdapterConfig<PatientImprovementGoal, PatientImprovementGoalDto>
            .NewConfig()
            .Map(dest => dest.GoalName, src => src.Goal.Name);

    }
}