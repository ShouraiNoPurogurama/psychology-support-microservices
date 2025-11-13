using BuildingBlocks.Messaging.Events.Queries.Subscription;
using MassTransit;
using Promotion.Grpc;
using Subscription.API.Data;
using System.Data;

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
                IgnoreExpired = false
            });
            if (promoResponse.PromoCode is null)
            {
                errors.Add("Promotion code is not valid");
            }
        }
        if (request.GiftId.HasValue && !string.IsNullOrWhiteSpace(request.GiftId.Value.ToString()))
        {
            var giftResponse = await promotionService.GetGiftCodeByPatientIdAsync(new GetGiftCodeByPatientIdRequest()
            {
                Id = request.PatientId.ToString()
            });
            appliedGift = giftResponse.GiftCode
                .FirstOrDefault(g => g.Id == request.GiftId.Value.ToString());
            if (appliedGift is null)
            {
                errors.Add("Gift code is not valid");
            }
        }
        var promoDeduction = (decimal?)(promoResponse?.PromoCode?.Value) ?? 0m;
        var giftDeduction = (decimal?)(appliedGift?.MoneyValue) ?? 0m;
        var calculatedFinalPrice = servicePackage.Price - promoDeduction - giftDeduction;
        calculatedFinalPrice = Math.Max(calculatedFinalPrice, 0);
        calculatedFinalPrice -= request.OldSubscriptionPrice;

        if (calculatedFinalPrice != request.FinalPrice - request.OldSubscriptionPrice)
        {
            errors.Add("Final price is not valid");
        }
        if (errors.Count == 0)
        {
            await context.RespondAsync(ValidateSubscriptionResponse.Success());
        }
        else
        {
            await context.RespondAsync(ValidateSubscriptionResponse.Failed(errors));
        }
    }
}