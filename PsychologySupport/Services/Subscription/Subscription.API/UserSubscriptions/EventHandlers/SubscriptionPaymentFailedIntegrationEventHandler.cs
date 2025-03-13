using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using MediatR;
using Promotion.Grpc;
using Subscription.API.Data;
using Subscription.API.Data.Common;
using Subscription.API.UserSubscriptions.Features.UpdateSubscriptionStatus;
using Subscription.API.UserSubscriptions.Features.UpdateUserSubscription;

namespace Subscription.API.UserSubscriptions.EventHandlers;

public class SubscriptionPaymentFailedIntegrationEventHandler(
    PromotionService.PromotionServiceClient promotionService,
    ISender sender)
    : IConsumer<SubscriptionPaymentDetailFailedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SubscriptionPaymentDetailFailedIntegrationEvent> context)
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