using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Payment;
using Mapster;
using MassTransit;
using Promotion.Grpc;
using Subscription.API.Data;
using Subscription.API.ServicePackages.Models;
using Subscription.API.UserSubscriptions.Dtos;
using Subscription.API.UserSubscriptions.Models;

namespace Subscription.API.UserSubscriptions.Features.CreateUserSubscription;

public record CreateUserSubscriptionCommand(CreateUserSubscriptionDto UserSubscription) : ICommand<CreateUserSubscriptionResult>;

public record CreateUserSubscriptionResult(Guid Id);

public class CreateUserSubscriptionHandler(
    SubscriptionDbContext context,
    IPublishEndpoint publishEndpoint,
    PromotionService.PromotionServiceClient promotionService)
    : ICommandHandler<CreateUserSubscriptionCommand, CreateUserSubscriptionResult>
{
    private readonly PromotionService.PromotionServiceClient _promotionService = promotionService;

    public async Task<CreateUserSubscriptionResult> Handle(CreateUserSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.UserSubscription;

        var servicePackage = await context.ServicePackages
                                 .FindAsync([dto.ServicePackageId], cancellationToken)
                             ?? throw new NotFoundException(nameof(ServicePackage), dto.ServicePackageId);

        //Calculate price after deducting promo code and gift code
        var (finalPrice, promotion) = await CalculateFinalPriceAsync(cancellationToken, servicePackage, dto);

        var userSubscription = UserSubscription.Create(dto.PatientId, dto.ServicePackageId, dto.StartDate, dto.EndDate,
            Guid.TryParse(promotion?.Id, out var id) ? id : null, dto.GiftId, servicePackage, finalPrice);

        context.UserSubscriptions.Add(userSubscription);
        await context.SaveChangesAsync(cancellationToken);

        //Publish event to Payment
        var subscriptionCreatedEvent = dto.Adapt<UserSubscriptionCreatedIntegrationEvent>();
        servicePackage.Adapt(subscriptionCreatedEvent);
        subscriptionCreatedEvent.SubscriptionId = userSubscription.Id;

        await publishEndpoint.Publish(subscriptionCreatedEvent, cancellationToken);

        return new CreateUserSubscriptionResult(userSubscription.Id);
    }

    private async Task<(decimal finalPrice, PromoCodeActivateDto? promotion)> CalculateFinalPriceAsync(
        CancellationToken cancellationToken, 
        ServicePackage servicePackage,
        CreateUserSubscriptionDto dto)
    {
        var finalPrice = servicePackage.Price;

        var promotion = (await _promotionService.GetPromotionByCodeAsync(new GetPromotionByCodeRequest()
        {
            Code = dto.PromoCode
        }, cancellationToken: cancellationToken)).PromoCode;

        if (promotion is not null)
        {
            finalPrice *= promotion.Value;
        }

        if (dto.GiftId is not null)
        {
            var giftCode = (await _promotionService.GetGiftCodeByPatientIdAsync(new GetGiftCodeByPatientIdRequest
                {
                    Id = dto.PatientId.ToString()
                }, cancellationToken: cancellationToken))
                .GiftCode
                .FirstOrDefault(g => g.Id == dto.GiftId.ToString());

            if (giftCode is null) return (finalPrice, promotion);

            finalPrice -= (decimal)giftCode.MoneyValue;
            finalPrice = Math.Max(finalPrice, 0);
        }

        return (finalPrice, promotion);
    }
}