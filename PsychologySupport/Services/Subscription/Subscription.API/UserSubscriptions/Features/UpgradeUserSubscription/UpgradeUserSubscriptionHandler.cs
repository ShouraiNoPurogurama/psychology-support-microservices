using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Payment;
using BuildingBlocks.Messaging.Events.Profile;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Promotion.Grpc;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.ServicePackages.Models;
using Subscription.API.UserSubscriptions.Dtos;
using Subscription.API.UserSubscriptions.Models;

namespace Subscription.API.UserSubscriptions.Features.UpgradeUserSubscription;

public record UpgradeUserSubscriptionCommand(UpgradeUserSubscriptionDto UpgradeUserSubscriptionDto)
    : ICommand<UpgradeUserSubscriptionResult>;

public record UpgradeUserSubscriptionResult(Guid Id, string PaymentUrl);

public class UpgradeUserSubscriptionHandler(
    SubscriptionDbContext dbContext,
    IRequestClient<GenerateUpgradeSubscriptionPaymentUrlRequest> paymentClient,
    IRequestClient<GetPatientProfileRequest> getPatientProfileClient,
    PromotionService.PromotionServiceClient promotionService)
    : ICommandHandler<UpgradeUserSubscriptionCommand, UpgradeUserSubscriptionResult>
{
    public async Task<UpgradeUserSubscriptionResult> Handle(UpgradeUserSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var patient =
            await getPatientProfileClient.GetResponse<GetPatientProfileResponse>(
                new GetPatientProfileRequest(request.UpgradeUserSubscriptionDto.PatientId), cancellationToken);

        if (!patient.Message.PatientExists)
        {
            // throw new NotFoundException(request.UpgradeUserSubscriptionDto.PatientId);
        }

        //Validate if there is an existing subscription
        
        var dto = request.UpgradeUserSubscriptionDto;

        ServicePackage newServicePackage = await dbContext.ServicePackages
                                            .FindAsync([dto.NewServicePackageId], cancellationToken)
                                        ?? throw new NotFoundException(nameof(ServicePackage), dto.NewServicePackageId);

        var currentSubscription = await ValidateCurrentSubscription(request, cancellationToken, newServicePackage);
        
        #region Calculate price after deducting promo code and gift code

        var (priceAfterAppliedPromotions, promoCode) =
            await CalculatePriceAfterAppliedPromotionsAsync(cancellationToken, newServicePackage, dto);

        var currentSubscriptionPriceLeft =
             CalculatePriceDiff(currentSubscription);

        Guid.TryParse(promoCode?.Id, out var promoCodeId);

        var newUserSubscription = UserSubscription.Create(dto.PatientId, dto.NewServicePackageId, dto.StartDate, 
            promoCodeId, dto.GiftId, newServicePackage, priceAfterAppliedPromotions);
        
        dbContext.UserSubscriptions.Add(newUserSubscription);
        await dbContext.SaveChangesAsync(cancellationToken);

        #endregion

        #region Publish event to Payment

        var subscriptionCreatedEvent = dto.Adapt<GenerateUpgradeSubscriptionPaymentUrlRequest>();
        subscriptionCreatedEvent.PaymentType = PaymentType.UpgradeSubscription;
        subscriptionCreatedEvent.SubscriptionId = newUserSubscription.Id;
        subscriptionCreatedEvent.Name = newServicePackage.Name;
        subscriptionCreatedEvent.Description = newServicePackage.Description;
        subscriptionCreatedEvent.ServicePackageId = newServicePackage.Id;
        subscriptionCreatedEvent.PatientId = newUserSubscription.PatientId;
        subscriptionCreatedEvent.PatientEmail = patient.Message.Email;
        subscriptionCreatedEvent.DurationDays = newServicePackage.DurationDays;
        subscriptionCreatedEvent.FinalPrice = priceAfterAppliedPromotions;
        subscriptionCreatedEvent.PromoCode = promoCode?.Code;
        subscriptionCreatedEvent.OldSubscriptionPrice = currentSubscriptionPriceLeft;

        // await publishEndpoint.Publish(subscriptionCreatedEvent, cancellationToken);
        var paymentUrl =
            await paymentClient.GetResponse<GenerateUpgradeSubscriptionPaymentUrlResponse>(
                subscriptionCreatedEvent,
                cancellationToken);

        if (paymentUrl.Message is null)
            throw new BadRequestException("Cannot create payment url.");

        #endregion

        return new UpgradeUserSubscriptionResult(newUserSubscription.Id, paymentUrl.Message.Url);
    }

    private decimal CalculatePriceDiff(UserSubscription currentSubscription)
    {

        var remainingDays = (int)Math.Floor((currentSubscription.EndDate - DateTime.UtcNow).TotalDays);
        var currSubscriptionFinalPrice = currentSubscription.FinalPrice;

        decimal durationDays = currentSubscription.ServicePackage.DurationDays;

        decimal currSubscriptionPriceLeft = Math.Round(currSubscriptionFinalPrice * (remainingDays / durationDays), 2,
            MidpointRounding.AwayFromZero);
        
        return currSubscriptionPriceLeft;
    }

    private async Task<UserSubscription> ValidateCurrentSubscription(UpgradeUserSubscriptionCommand request, CancellationToken cancellationToken,
        ServicePackage newServicePackage)
    {
        var currentSubscription =
            await dbContext.UserSubscriptions
                .Include(u => u.ServicePackage)
                .FirstOrDefaultAsync(
                    x => x.PatientId == request.UpgradeUserSubscriptionDto.PatientId && x.Status == SubscriptionStatus.Active,
                    cancellationToken);

        if (currentSubscription == null)
        {
            throw new NotFoundException("The patient does not have an active subscription.");
        }
        
        if(currentSubscription.ServicePackage.Price > newServicePackage.Price)
            throw new BadRequestException("The new subscription price must be greater than the current subscription price.");
        
        return currentSubscription;
    }


    private async Task<(decimal finalPrice, PromoCodeActivateDto? promotion)> CalculatePriceAfterAppliedPromotionsAsync(
        CancellationToken cancellationToken,
        ServicePackage servicePackage,
        UpgradeUserSubscriptionDto dto)
    {
        var finalPrice = servicePackage.Price;

        if (string.IsNullOrEmpty(dto.PromoCode) && string.IsNullOrEmpty(dto.GiftId.ToString()))
            return (finalPrice, null);

        //Apply promotion
        var promotion = (await promotionService.GetPromotionByCodeAsync(new GetPromotionByCodeRequest
        {
            Code = dto.PromoCode,
            IgnoreExpired = false
        }, cancellationToken: cancellationToken)).PromoCode;

        if (promotion is not null)
        {
            finalPrice *= 0.01m * promotion.Value;
            await promotionService.ConsumePromoCodeAsync(new ConsumePromoCodeRequest
            {
                PromoCodeId = promotion.Id
            });
        }

        //Apply Gift
        if (dto.GiftId is null) return (finalPrice, promotion);

        var giftCode = (await promotionService.GetGiftCodeByPatientIdAsync(new GetGiftCodeByPatientIdRequest
            {
                Id = dto.PatientId.ToString()
            }, cancellationToken: cancellationToken))
            .GiftCode
            .FirstOrDefault(g => g.Id == dto.GiftId.ToString());

        if (giftCode is null) return (finalPrice, promotion);

        finalPrice -= (decimal)giftCode.MoneyValue;
        finalPrice = Math.Max(finalPrice, 0);

        await promotionService.ConsumeGiftCodeAsync(new ConsumeGiftCodeRequest
        {
            GiftCodeId = giftCode.Id
        });

        return (finalPrice, promotion);
    }
}