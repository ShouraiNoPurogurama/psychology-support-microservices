using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Payment;
using BuildingBlocks.Messaging.Events.Profile;
using Mapster;
using MassTransit;
using Profile.API.PatientProfiles.Models;
using Promotion.Grpc;
using Subscription.API.Data;
using Subscription.API.ServicePackages.Models;
using Subscription.API.UserSubscriptions.Dtos;
using Subscription.API.UserSubscriptions.Models;

namespace Subscription.API.UserSubscriptions.Features.CreateUserSubscription;

public record CreateUserSubscriptionCommand(CreateUserSubscriptionDto UserSubscription) : ICommand<CreateUserSubscriptionResult>;

public record CreateUserSubscriptionResult(Guid Id, string PaymentUrl);

public class CreateUserSubscriptionHandler(
    SubscriptionDbContext context,
    IRequestClient<GeneratePaymentUrlRequest> generatePaymentUrlClient,
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
            throw new NotFoundException(nameof(PatientProfile), request.UserSubscription.PatientId);
        }

        var dto = request.UserSubscription;

        ServicePackage servicePackage = await context.ServicePackages
                                            .FindAsync([dto.ServicePackageId], cancellationToken)
                                        ?? throw new NotFoundException(nameof(ServicePackage), dto.ServicePackageId);

        #region Calculate price after deducting promo code and gift code

        var (finalPrice, promoCode) = await CalculateFinalPriceAsync(cancellationToken, servicePackage, dto);

        Guid.TryParse(promoCode?.Id, out var promoCodeId);

        var userSubscription = UserSubscription.Create(dto.PatientId, dto.ServicePackageId, dto.StartDate, dto.EndDate,
            promoCodeId, dto.GiftId, servicePackage, finalPrice);

        context.UserSubscriptions.Add(userSubscription);
        await context.SaveChangesAsync(cancellationToken);

        #endregion

        #region Publish event to Payment

        var subscriptionCreatedEvent = dto.Adapt<GeneratePaymentUrlRequest>();
        subscriptionCreatedEvent.SubscriptionId = userSubscription.Id;
        subscriptionCreatedEvent.ServicePackageId = servicePackage.Id;
        subscriptionCreatedEvent.PatientId = userSubscription.PatientId;
        subscriptionCreatedEvent.PatientEmail = patient.Message.Email;
        subscriptionCreatedEvent.DurationDays = servicePackage.DurationDays;
        subscriptionCreatedEvent.FinalPrice = finalPrice;
        subscriptionCreatedEvent.PromoCode = promoCode?.Code;

        #endregion

        // await publishEndpoint.Publish(subscriptionCreatedEvent, cancellationToken);
        var paymentUrl =
            await generatePaymentUrlClient.GetResponse<GeneratePaymentUrlResponse>(
                subscriptionCreatedEvent.Adapt<GeneratePaymentUrlRequest>(), cancellationToken);

        if (paymentUrl.Message is null)
            throw new BadRequestException("Cannot create payment url.");

        return new CreateUserSubscriptionResult(userSubscription.Id, paymentUrl.Message.Url);
    }

    private async Task<(decimal finalPrice, PromoCodeActivateDto? promotion)> CalculateFinalPriceAsync(
        CancellationToken cancellationToken,
        ServicePackage servicePackage,
        CreateUserSubscriptionDto dto)
    {
        var finalPrice = servicePackage.Price;

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