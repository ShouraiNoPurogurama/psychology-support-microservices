using System.Collections;
using System.Reflection;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Mapster;
using Promotion.Grpc.Models;

namespace Promotion.Grpc.Extensions;

public static class MapsterConfigurations
{
    public static void RegisterMapsterConfigurations(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MapsterConfigurations).Assembly);
        
        TypeAdapterConfig.GlobalSettings.Default
            .UseDestinationValue(member => member.SetterModifier == AccessModifier.None &&
                                           member.Type.IsGenericType &&
                                           member.Type.GetGenericTypeDefinition() == typeof(RepeatedField<>));

        TypeAdapterConfig<Models.Promotion, PromotionDto>.NewConfig()
            .Map(dest => dest.StartDate, src => src.EffectiveDate.ToTimestamp())
            .Map(dest => dest.EndDate, src => src.EndDate.ToTimestamp());


    }
}
