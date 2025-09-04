using System.Data;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Queries.Payment;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using BuildingBlocks.Messaging.Events.Queries.Subscription;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Promotion.Grpc;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.ServicePackages.Models;
using Subscription.API.UserSubscriptions.Dtos;
using Subscription.API.UserSubscriptions.Models;
// using Profile.API.PatientProfiles.Models;

namespace Subscription.API.UserSubscriptions.Features.v2.CreateUserSubscription;

public record CreateUserSubscriptionCommand(CreateUserSubscriptionDto UserSubscription) : ICommand<CreateUserSubscriptionResult>;

public record CreateUserSubscriptionResult(Guid Id, string PaymentUrl, long? PaymentCode);

public class CreateUserSubscriptionHandler(
    SubscriptionDbContext context,
    IRequestClient<GenerateSubscriptionPaymentUrlRequest> paymentClient,
    IRequestClient<GetPatientProfileRequest> getPatientProfileClient,
    IRequestClient<GetPendingPaymentUrlForSubscriptionRequest> paymentUrlClient,
    PromotionService.PromotionServiceClient promotionService)
    : ICommandHandler<CreateUserSubscriptionCommand, CreateUserSubscriptionResult>
{
    public async Task<CreateUserSubscriptionResult> Handle(CreateUserSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var patient =
            await getPatientProfileClient.GetResponse<GetPatientProfileResponse>(
                new GetPatientProfileRequest(request.UserSubscription.PatientId), cancellationToken);

        if (!patient.Message.PatientExists)
        {
            // throw new NotFoundException(nameof(PatientProfile), request.UserSubscription.PatientId);
        }

        //Check if there is an existing subscription
        var hasActive = await context.UserSubscriptions
            .AnyAsync(x => x.PatientId == request.UserSubscription.PatientId && x.Status == SubscriptionStatus.Active, cancellationToken);

        if (hasActive)
            throw new BadRequestException("Patient already has an active subscription.");

        //Check if user have any awaiting payment
        var awaitingPaymentSubscription = await context.UserSubscriptions
            .Include(x => x.ServicePackage)
            .FirstOrDefaultAsync(x => x.PatientId == request.UserSubscription.PatientId &&
                                      x.Status == SubscriptionStatus.AwaitPayment, cancellationToken: cancellationToken);
        
        if (awaitingPaymentSubscription is not null)
        {
            bool isDifferentService = awaitingPaymentSubscription.ServicePackageId != request.UserSubscription.ServicePackageId;

            if (isDifferentService)
            {
                throw new BadRequestException($"Patient already has an awaiting payment subscription for another service package: {awaitingPaymentSubscription.ServicePackage.Name}.");
            }
            
            var paymentUrlResponse =
                await paymentUrlClient.GetResponse<GetPendingPaymentUrlForSubscriptionResponse>(
                    new GetPendingPaymentUrlForSubscriptionRequest(awaitingPaymentSubscription.Id), cancellationToken);
            
            var existingPaymentUrl = paymentUrlResponse.Message.Url ??
                                     throw new BadRequestException(
                                         "Cannot create payment url for awaiting payment subscription.");
            var existingPaymentCode = paymentUrlResponse.Message.PaymentCode ??
                                     throw new BadRequestException(
                                         "Cannot create payment code for awaiting payment subscription.");

            return new CreateUserSubscriptionResult(awaitingPaymentSubscription.Id, existingPaymentUrl,existingPaymentCode);
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

        var paymentUrl = await GeneratePaymentUrl(cancellationToken, dto, userSubscription, servicePackage, patient, finalPrice,
            promoCode);
        
        await transaction.CommitAsync(cancellationToken);
        
        return new CreateUserSubscriptionResult(userSubscription.Id, paymentUrl.Message.Url, paymentUrl.Message.PaymentCode);
    }

    private async Task<Response<GenerateSubscriptionPaymentUrlResponse>> GeneratePaymentUrl(CancellationToken cancellationToken,
        CreateUserSubscriptionDto dto,
        UserSubscription userSubscription, ServicePackage servicePackage, Response<GetPatientProfileResponse> patient,
        decimal finalPrice,
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
        subscriptionCreatedEvent.ServicePackageName = servicePackage.Name;

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