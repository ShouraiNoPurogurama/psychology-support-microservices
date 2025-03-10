using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using Promotion.Grpc;

namespace Subscription.API.UserSubscriptions.EventHandlers;

public class SubscriptionPaymentFailedIntegrationEventHandler(PromotionService.PromotionServiceClient promotionService)
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
    }
}