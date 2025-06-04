using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Payment;
using BuildingBlocks.Messaging.Events.Profile;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
// using Profile.API.PatientProfiles.Models;
using Promotion.Grpc;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.ServicePackages.Models;
using Subscription.API.UserSubscriptions.Dtos;
using Subscription.API.UserSubscriptions.Models;

namespace Subscription.API.UserSubscriptions.Features.CreateUserSubscription;

public record CreateUserSubscriptionCommand(CreateUserSubscriptionDto UserSubscription) : ICommand<CreateUserSubscriptionResult>;

public record CreateUserSubscriptionResult(Guid Id, string PaymentUrl);

public class CreateUserSubscriptionHandler(
    SubscriptionDbContext context,
    IRequestClient<GenerateSubscriptionPaymentUrlRequest> paymentClient,
    IRequestClient<GetPatientProfileRequest> getPatientProfileClient,
    PromotionService.PromotionServiceClient promotionService)
    : ICommandHandler<CreateUserSubscriptionCommand, CreateUserSubscriptionResult>
{
    public async Task<CreateUserSubscriptionResult> Handle(CreateUserSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var patient =
            await getPatientProfileClient.GetResponse<GetPatientProfileResponse>(
                new GetPatientProfileRequest(request.UserSubscription.PatientId), cancellationToken);

        if (!patient.Message.PatientExists)
        {
            // throw new NotFoundException(nameof(PatientProfile), request.UserSubscription.PatientId);
        }

        //Check if there is an existing subscription
        var existingSubscription = context.UserSubscriptions
            .Any(x => x.PatientId == request.UserSubscription.PatientId && x.Status == SubscriptionStatus.Active );
        
        if (existingSubscription)
            throw new BadRequestException("Patient already has an active subscription.");

        //Check if user have any awaiting payment
        var awaitingPaymentSubscription = await context.UserSubscriptions
            .FirstOrDefaultAsync(x => x.PatientId == request.UserSubscription.PatientId &&
                                      x.Status == SubscriptionStatus.AwaitPayment, cancellationToken: cancellationToken);
        
        if (awaitingPaymentSubscription is not null)
        {
            var paymentUrlResponse =
                await paymentClient.GetResponse<GetPendingPaymentUrlForSubscriptionResponse>(
                    new GetPendingPaymentUrlForSubscriptionRequest(awaitingPaymentSubscription.Id), cancellationToken);

            var existingPaymentUrl = paymentUrlResponse.Message.Url ??
                                     throw new BadRequestException(
                                         "Cannot create payment url for awaiting payment subscription.");

            return new CreateUserSubscriptionResult(awaitingPaymentSubscription.Id, existingPaymentUrl);
        }

        var dto = request.UserSubscription;

        ServicePackage servicePackage = await context.ServicePackages
                                            .FindAsync([dto.ServicePackageId], cancellationToken)
                                        ?? throw new NotFoundException(nameof(ServicePackage), dto.ServicePackageId);

        #region Calculate price after deducting promo code and gift code

        var (finalPrice, promoCode) = await CalculateFinalPriceAsync(cancellationToken, servicePackage, dto);

        Guid.TryParse(promoCode?.Id, out var promoCodeId);

        var userSubscription = UserSubscription.Create(dto.PatientId, dto.ServicePackageId, dto.StartDate,
            promoCodeId, dto.GiftId, servicePackage, finalPrice);

        
        context.UserSubscriptions.Add(userSubscription);
        await context.SaveChangesAsync(cancellationToken);

        #endregion
        
        var paymentUrl = await GeneratePaymentUrl(cancellationToken, dto, userSubscription, servicePackage, patient, finalPrice, promoCode);
        
        return new CreateUserSubscriptionResult(userSubscription.Id, paymentUrl.Message.Url);
    }

    private async Task<Response<GenerateSubscriptionPaymentUrlResponse>> GeneratePaymentUrl(CancellationToken cancellationToken, CreateUserSubscriptionDto dto,
        UserSubscription userSubscription, ServicePackage servicePackage, Response<GetPatientProfileResponse> patient, decimal finalPrice,
        PromoCodeActivateDto? promoCode)
    {
        GenerateSubscriptionPaymentUrlRequest subscriptionCreatedEvent = dto.Adapt<GenerateSubscriptionPaymentUrlRequest>();
        subscriptionCreatedEvent.SubscriptionId = userSubscription.Id;
        subscriptionCreatedEvent.Name = servicePackage.Name;
        subscriptionCreatedEvent.Description = servicePackage.Description;
        subscriptionCreatedEvent.ServicePackageId = servicePackage.Id;
        subscriptionCreatedEvent.PatientId = userSubscription.PatientId;
        subscriptionCreatedEvent.PatientEmail = patient.Message.Email;
        subscriptionCreatedEvent.DurationDays = servicePackage.DurationDays;
        subscriptionCreatedEvent.FinalPrice = finalPrice;
        subscriptionCreatedEvent.PromoCode = promoCode?.Code;

        // await publishEndpoint.Publish(subscriptionCreatedEvent, cancellationToken);
        var paymentUrl =
            await paymentClient.GetResponse<GenerateSubscriptionPaymentUrlResponse>(
                subscriptionCreatedEvent.Adapt<GenerateSubscriptionPaymentUrlRequest>(), cancellationToken);

        if (paymentUrl.Message is null)
            throw new BadRequestException("Cannot create payment url.");
        return paymentUrl;
    }

    private async Task<(decimal finalPrice, PromoCodeActivateDto? promotion)> CalculateFinalPriceAsync(
        CancellationToken cancellationToken,
        ServicePackage servicePackage,
        CreateUserSubscriptionDto dto)
    {
        var finalPrice = servicePackage.Price;

        if (string.IsNullOrEmpty(dto.PromoCode) && string.IsNullOrEmpty(dto.GiftId.ToString()))
            return (finalPrice, null);

        //Apply promotion
        var promotion = (await promotionService.GetPromotionByCodeAsync(new GetPromotionByCodeRequest()
        {
            Code = dto.PromoCode,
            IgnoreExpired = false
        }, cancellationToken: cancellationToken)).PromoCode;

        if (promotion is not null)
        {
            finalPrice *= 0.01m * promotion.Value;
            await promotionService.ConsumePromoCodeAsync(new ConsumePromoCodeRequest()
            {
                PromoCodeId = promotion.Id,
            });
        }

        //Apply Gift
        if (dto.GiftId is null) return (finalPrice, promotion);

        var giftCode = (await promotionService.GetGiftCodeByPatientIdAsync(new GetGiftCodeByPatientIdRequest()
            {
                Id = dto.PatientId.ToString()
            }, cancellationToken: cancellationToken))
            .GiftCode
            .FirstOrDefault(g => g.Id == dto.GiftId.ToString());

        if (giftCode is null) return (finalPrice, promotion);

        finalPrice -= (decimal)giftCode.MoneyValue;
        finalPrice = Math.Max(finalPrice, 0);

        await promotionService.ConsumeGiftCodeAsync(new ConsumeGiftCodeRequest()
        {
            GiftCodeId = giftCode.Id
        });

        return (finalPrice, promotion);
    }
}