using Google.Protobuf.Collections;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Promotion.Grpc.Data;
using Promotion.Grpc.Models;

namespace Promotion.Grpc.Services;

public class PromotionService(PromotionDbContext dbContext, ValidatorService validator)
    : Grpc.PromotionService.PromotionServiceBase
{
    public override async Task<GetPromotionsByServicePackageIdResponse> GetPromotionsByServicePackageId(
        GetPromotionsByServicePackageIdRequest request, ServerCallContext context)
    {
        validator.ValidateGuid(request.ServicePackageId, "Service Package");

        var promotionTypes = dbContext.PromotionTypeServicePackages
            .Include(p => p.PromotionType.Promotions)
            .Where(p => p.ServicePackageId.Equals(request.ServicePackageId))
            .Select(p => p.PromotionType)
            .AsQueryable();

        //Fetch all promotions associate with the types
        var promotions = await promotionTypes
            .SelectMany(t => t.Promotions)
            .Where(p => p.IsActive == true)
            .Include(p => p.PromoCodes)
            .Include(p => p.GiftCodes)
            .ToDictionaryAsync(p => p.Id, p => p);

        var promotionDtos = promotions.Values.Adapt<RepeatedField<PromotionDto>>();

        PopulatePromoCodesAndGiftCodes(promotionDtos, promotions);

        return new GetPromotionsByServicePackageIdResponse
        {
            Promotions = { promotionDtos }
        };
    }

    private static void PopulatePromoCodesAndGiftCodes(RepeatedField<PromotionDto> promotionDtos,
        Dictionary<string, Models.Promotion> promotions)
    {
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
    }

    public override async Task<GetPromotionByCodeResponse> GetPromotionByCode(GetPromotionByCodeRequest request,
        ServerCallContext context)
    {
        PromoCode promoCode = await dbContext.PromoCodes
                                  .Include(p => p.Promotion)
                                  .ThenInclude(p => p.PromotionType)
                                  .Where(p => p.Promotion.IsActive == true && p.IsActive == true)
                                  .FirstOrDefaultAsync(p => p.Code.Equals(request.Code.Trim()))
                              ?? throw new RpcException(new Status(StatusCode.NotFound, "Promo code not found"));

        return new GetPromotionByCodeResponse
        {
            PromoCode = promoCode.Adapt<PromoCodeActivateDto>()
        };
    }

    public override async Task<GetGiftCodeByPatientIdResponse> GetGiftCodeByPatientId(GetGiftCodeByPatientIdRequest request,
        ServerCallContext context)
    {
        validator.ValidateGuid(request.Id, "Patient Id");

        var giftCode = await dbContext.GiftCodes
            .Include(g => g.Promotion)
            .ThenInclude(p => p.PromotionType)
            .Where(g => g.PatientId.Equals(request.Id.Trim())
                        && g.Promotion.IsActive == true && g.IsActive == true)
            .ToListAsync();

        var giftCodeDtos = giftCode.Adapt<RepeatedField<GiftCodeActivateDto>>();

        var response = new GetGiftCodeByPatientIdResponse();

        response.GiftCode.AddRange(giftCodeDtos);

        return response;
    }

    public override async Task<PromotionDto> CreatePromotion(CreatePromotionRequest request, ServerCallContext context)
    {
        validator.ValidateCreatePromotionRequest(request);

        var promotion = request.Adapt<Models.Promotion>();

        var promotionType = await dbContext.PromotionTypes.FindAsync(request.PromotionTypeId)
                            ?? throw new RpcException(new Status(StatusCode.NotFound, "Promotion type not found"));

        promotion.Id = Guid.NewGuid().ToString();
        promotion.PromotionType = promotionType;
        //Temporary hard code the ImgId
        promotion.ImageId = Guid.NewGuid().ToString();

        AddPromoCodesToPromotion(request.CreatePromoCodesDto, promotion);

        dbContext.Promotions.Add(promotion);
        await dbContext.SaveChangesAsync();

        var promotionDto = promotion.Adapt<PromotionDto>();
        promotionDto.PromoCodes.AddRange(promotion.PromoCodes.Adapt<RepeatedField<PromoCodeDto>>());

        return promotionDto;
    }

    private static void AddPromoCodesToPromotion(CreatePromoCodesDto createPromoCodesDto, Models.Promotion promotion)
    {
        for (int i = 0; i < createPromoCodesDto.Quantity; i++)
        {
            var newPromoCode = createPromoCodesDto.Adapt<PromoCode>();
            newPromoCode.PromotionId = promotion.Id;
            promotion.PromoCodes.Add(newPromoCode);
        }
    }


    public override async Task<UpdatePromotionResponse> UpdatePromotion(UpdatePromotionRequest request, ServerCallContext context)
    {
        validator.ValidateUpdatePromotionRequest(request);

        var promotion = await dbContext.Promotions.FindAsync(request.PromotionId)
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Promotion not found"));

        promotion.EffectiveDate = request.StartDate.ToDateTime();
        promotion.EndDate = request.EndDate.ToDateTime();

        await dbContext.SaveChangesAsync();

        return new UpdatePromotionResponse()
        {
            IsSuccess = true
        };
    }

    public override async Task<AddPromoCodesToPromotionResponse> AddPromoCodesToPromotion(AddPromoCodesToPromotionRequest request,
        ServerCallContext context)
    {
        validator.ValidateGuid(request.PromotionId, "Promotion");

        var promotion = dbContext.Promotions
                            .Include(p => p.PromoCodes)
                            .FirstOrDefault(p => p.Id.Equals(request.PromotionId))
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Promotion not found"));

        AddPromoCodesToPromotion(request.PromoCode, promotion);

        await dbContext.SaveChangesAsync();

        return new AddPromoCodesToPromotionResponse
        {
            IsSuccess = true
        };
    }

    public override async Task<AddGiftCodesToPromotionResponse> AddGiftCodesToPromotion(AddGiftCodesToPromotionRequest request,
        ServerCallContext context)
    {
        var promotion = await dbContext.Promotions.FindAsync(request.PromotionId)
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Promotion not found"));

        var giftCode = request.CreateGiftCodeDto.Adapt<GiftCode>();

        promotion.GiftCodes.Add(giftCode);

        dbContext.GiftCodes.Add(giftCode);

        await dbContext.SaveChangesAsync();

        return new AddGiftCodesToPromotionResponse
        {
            IsSuccess = true
        };
    }

    public override async Task<DeletePromotionResponse> DeletePromotion(DeletePromotionRequest request, ServerCallContext context)
    {
        var promotion = await dbContext.Promotions.FindAsync(request.PromotionId)
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Promotion not found"));

        promotion.IsActive = false;

        return new DeletePromotionResponse()
        {
            IsSuccess = true
        };
    }

    public override async Task<ConsumePromoCodeResponse> ConsumePromoCode(ConsumePromoCodeRequest request,
        ServerCallContext context)
    {
        var promoCode = await dbContext.PromoCodes
                            .FindAsync(request.PromoCodeId)
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Promo code not found"));

        promoCode.IsActive = false;

        await dbContext.SaveChangesAsync();

        return new ConsumePromoCodeResponse()
        {
            IsSuccess = true
        };
    }

    public override async Task<ConsumeGiftCodeResponse> ConsumeGiftCode(ConsumeGiftCodeRequest request, ServerCallContext context)
    {
        var promoCode = await dbContext.GiftCodes
                            .FindAsync(request.GiftCodeId)
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Gift code not found"));

        promoCode.IsActive = false;

        await dbContext.SaveChangesAsync();

        return new ConsumeGiftCodeResponse()
        {
            IsSuccess = true
        };
    }
}