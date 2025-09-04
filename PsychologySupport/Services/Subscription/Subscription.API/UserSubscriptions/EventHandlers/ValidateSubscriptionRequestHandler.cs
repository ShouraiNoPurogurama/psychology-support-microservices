using BuildingBlocks.Messaging.Events.Queries.Subscription;
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

        GetPromotionByCodeResponse? promoResponse = null;
        GiftCodeActivateDto? appliedGift = null;
        List<string> errors = [];

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
        }

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            promoResponse = await promotionService.GetPromotionByCodeAsync(new GetPromotionByCodeRequest
            {
                Code = request.PromoCode,
                IgnoreExpired = true
            });

            if (promoResponse.PromoCode is null)
            {
                errors.Add("Promotion code is not valid");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.GiftId.ToString()))
        {
            var giftResponse = await promotionService.GetGiftCodeByPatientIdAsync(new GetGiftCodeByPatientIdRequest()
            {
                Id = request.PatientId.ToString()
            });

            appliedGift = giftResponse.GiftCode
                .FirstOrDefault(g => g.Id != request.GiftId.ToString());

            if (appliedGift is null)
            {
                errors.Add("Gift code is not valid");
            }
        }

        var calculatedFinalPrice = servicePackage.Price
                         - (promoResponse?.PromoCode != null ? 0.01m * promoResponse.PromoCode.Value * servicePackage.Price : 0)
                         - (appliedGift != null ? (decimal)appliedGift.MoneyValue : 0);

        calculatedFinalPrice -= request.OldSubscriptionPrice;
        
        if (calculatedFinalPrice != request.FinalPrice - request.OldSubscriptionPrice)
        {
            errors.Add("Final price is not valid");
        }

        switch (errors.Count == 0)
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