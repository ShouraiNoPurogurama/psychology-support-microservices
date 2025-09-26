using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Domain.Aggregates.Challenges;

namespace Wellness.Application.Extensions
{
    public static class MapsterConfigurations
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            // Scan the assembly for other mappings
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            TypeAdapterConfig<Activity, ActivityDto>
                .NewConfig()
                .Map(dest => dest.ActivityType, src => src.ActivityType.ToString());

            TypeAdapterConfig<ChallengeStep, ChallengeStepDto>
                .NewConfig()
                .Map(dest => dest.Activity, src => src.Activity);

            TypeAdapterConfig<Challenge, ChallengeDto>
                .NewConfig()
                .Map(dest => dest.ChallengeType, src => src.ChallengeType.ToString())
                .Map(dest => dest.Steps, src => src.ChallengeSteps);
        }
    }
}
