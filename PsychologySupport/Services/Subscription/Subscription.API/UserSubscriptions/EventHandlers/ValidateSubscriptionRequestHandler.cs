using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using Promotion.Grpc;
using Subscription.API.Data;

namespace Subscription.API.UserSubscriptions.EventHandlers;

public class ValidateSubscriptionRequestHandler(
    SubscriptionDbContext dbContext,
    PromotionService.PromotionServiceClient promotionService) : IConsumer<ValidateSubscriptionRequest>
{
    public async Task Consume(ConsumeContext<ValidateSubscriptionRequest> context)
    {
        var request = context.Message;
        //Validate Service Package(Duration, Price)
        //Validate Promotion Code
        //Validate Gift
        //Validate Final Price

        GetPromotionByCodeResponse? promotion = null;
        GiftCodeActivateDto? appliedGift = null;
        List<string> errors = [];
        bool isSuccess = true;

        var servicePackage = await dbContext.ServicePackages.FindAsync(request.ServicePackageId);

        if (servicePackage is null)
        {
            errors.Add("Service package not found");
            await context.RespondAsync(ValidateSubscriptionResponse.Failed(errors));
            return;
        }
        
        if (request.DurationDays != servicePackage.DurationDays)
        {
            errors.Add("Invalid duration days");
            isSuccess = false;
        }

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            promotion = await promotionService.GetPromotionByCodeAsync(new GetPromotionByCodeRequest
            {
                Code = request.PromoCode,
                IgnoreExpired = true
            });

            if (promotion.PromoCode is null)
            {
                errors.Add("Promotion code is not valid");
                isSuccess = false;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.GiftId.ToString()))
        {
            var gifts = await promotionService.GetGiftCodeByPatientIdAsync(new GetGiftCodeByPatientIdRequest()
            {
                Id = request.PatientId.ToString()
            });

            appliedGift = gifts.GiftCode
                .FirstOrDefault(g => g.Id != request.GiftId.ToString());

            if (appliedGift is null)
            {
                errors.Add("Gift code is not valid");
                isSuccess = false;
            }
        }

        var finalPrice = servicePackage.Price
                         - (promotion?.PromoCode != null ? 0.01m * promotion.PromoCode.Value * servicePackage.Price : 0)
                         - (appliedGift != null ? (decimal)appliedGift.MoneyValue : 0);

        if (finalPrice != request.FinalPrice)
        {
            errors.Add("Final price is not valid");
        }

        switch (isSuccess)
        {
            case true:
                await context.RespondAsync(ValidateSubscriptionResponse.Success());
                break;
            case false:
                await context.RespondAsync(ValidateSubscriptionResponse.Failed(errors));
                break;
        }
    }
}