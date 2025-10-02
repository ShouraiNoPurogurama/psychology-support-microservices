using BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;
using MassTransit;
using MediatR;
using Promotion.Grpc;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.UserSubscriptions.Features.v2.UpdateSubscriptionStatus;

namespace Subscription.API.UserSubscriptions.EventHandlers;

public class SubscriptionPaymentFailedIntegrationEventHandler(
    PromotionService.PromotionServiceClient promotionService,
    ISender sender)
    : IConsumer<SubscriptionPaymentFailedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SubscriptionPaymentFailedIntegrationEvent> context)
    {
        var message = context.Message;

        var giftId = message.GiftId.ToString();

        if (!string.IsNullOrWhiteSpace(message.PromoCode))
        {
            await promotionService.ReactivatePromoCodeAsync(new ReactivatePromoCodeRequest()
            {
                PromoCode = message.PromoCode
            });
        }

        if (!string.IsNullOrWhiteSpace(giftId))
        {
            await promotionService.ReactivateGiftCodeAsync(new ReactivateGiftCodeRequest()
            {
                GiftId = giftId
            });
        }

        await sender.Send(new UpdateUserSubscriptionStatusCommand(message.SubscriptionId, SubscriptionStatus.Cancelled));
    }
}