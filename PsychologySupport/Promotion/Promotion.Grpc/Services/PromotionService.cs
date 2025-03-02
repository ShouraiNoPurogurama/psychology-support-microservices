using Google.Protobuf.Collections;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Promotion.Grpc.Data;

namespace Promotion.Grpc.Services;

public class PromotionService(PromotionDbContext dbContext) : Grpc.PromotionService.PromotionServiceBase
{
    public override async Task<GetPromotionsByServicePackageIdResponse> GetPromotionsByServicePackageId(
        GetPromotionsByServicePackageIdRequest request, ServerCallContext context)
    {
        var promotionTypes = dbContext.PromotionTypeServicePackages
            .Include(p => p.PromotionType.Promotions)
            .Where(p => p.ServicePackageId.Equals(request.ServicePackageId))
            .Select(p => p.PromotionType)
            .AsQueryable();

        //Fetch all promotions associate with the types
        var promotions = await promotionTypes
            .SelectMany(t => t.Promotions)
            .Include(p => p.PromoCodes)
            .Include(p => p.GiftCodes)
            .ToDictionaryAsync(p => p.Id, p => p);

        var promotionDtos = promotions.Values.Adapt<RepeatedField<PromotionDto>>();

        //Populate RepeatedFields
        foreach (var promotionDto in promotionDtos)
        {
            promotionDto.PromoCodes.AddRange(promotions
                .GetValueOrDefault(promotionDto.Id)
                ?
                .PromoCodes.Adapt<RepeatedField<PromoCodeDto>>() ?? []);

            promotionDto.GiftCodes.AddRange(promotions
                .GetValueOrDefault(promotionDto.Id)
                ?
                .GiftCodes.Adapt<RepeatedField<GiftCodeDto>>() ?? []);
        }

        return new GetPromotionsByServicePackageIdResponse
        {
            Promotions = { promotionDtos }
        };
    }

    public override Task<PromotionDto> CreatePromotion(CreatePromotionRequest request, ServerCallContext context)
    {
        return base.CreatePromotion(request, context);
    }

    public override Task<PromotionDto> UpdatePromotion(UpdatePromotionRequest request, ServerCallContext context)
    {
        return base.UpdatePromotion(request, context);
    }

    public override Task<PromotionDto> AddPromoCodesToPromotion(AddPromoCodesToPromotionRequest request,
        ServerCallContext context)
    {
        return base.AddPromoCodesToPromotion(request, context);
    }

    public override Task<PromotionDto> AddGiftCodesToPromotion(AddGiftCodesToPromotionRequest request, ServerCallContext context)
    {
        return base.AddGiftCodesToPromotion(request, context);
    }

    public override Task<DeletePromotionResponse> DeletePromotion(DeletePromotionRequest request, ServerCallContext context)
    {
        return base.DeletePromotion(request, context);
    }
}