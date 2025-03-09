using System.Collections;
using System.Reflection;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Mapster;
using Promotion.Grpc.Models;
using Promotion.Grpc.Utils;

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
            .Map(dest => dest.EndDate, src => src.EndDate.ToTimestamp())
            .Ignore(dest => dest.GiftCodes)
            .Ignore(dest => dest.PromoCodes);

        TypeAdapterConfig<PromoCode, PromoCodeActivateDto>.NewConfig()
            .Map(dest => dest.PromotionType, src => src.Promotion.PromotionType);
        
        TypeAdapterConfig<GiftCode, GiftCodeActivateDto>.NewConfig()
            .Map(dest => dest.PromotionType, src => src.Promotion.PromotionType);
        
        TypeAdapterConfig<CreatePromotionRequest, Models.Promotion>.NewConfig()
            .Map(dest => dest.EffectiveDate, src => src.StartDate.ToDateTime())
            .Map(dest => dest.EndDate, src => src.EndDate.ToDateTime());

        TypeAdapterConfig<CreatePromoCodesDto, PromoCode>.NewConfig()
            .Map(dest => dest.Id, src => Guid.NewGuid().ToString())
            .Map(dest => dest.IsActive, src => true)
            .Map(dest => dest.Code, src => CommonUtils.GeneratePromoCode());

        TypeAdapterConfig<CreateGiftCodeDto, GiftCode>.NewConfig()
            .Map(dest => dest.Id, src => Guid.NewGuid().ToString());
    }
}
