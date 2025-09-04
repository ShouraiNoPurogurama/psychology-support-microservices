using BuildingBlocks.Messaging.Events.Queries.Subscription;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Promotion.Grpc;
using Subscription.API.Data;

namespace Subscription.API.UserSubscriptions.EventHandlers
{
    public class SubscriptionGetPromoAndGiftHandler(
        SubscriptionDbContext dbContext,
        PromotionService.PromotionServiceClient promotionService
    ) : IConsumer<SubscriptionGetPromoAndGiftRequest>
    {
        public async Task Consume(ConsumeContext<SubscriptionGetPromoAndGiftRequest> context)
        {
            var request = context.Message;

            var subscription = await dbContext.UserSubscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SubscriptionId);

            if (subscription == null)
            {
                await context.RespondAsync(new SubscriptionGetPromoAndGiftResponse(null, null));
                return;
            }

            string? promoCode = null;

            try
            {
                var promoResponse = await promotionService.GetPromotionByIdAsync(new Promotion.Grpc.GetPromotionByIdRequest
                {
                    PromotionId = subscription.PromoCodeId?.ToString() ?? Guid.Empty.ToString()
                });

                promoCode = promoResponse.Promotion
                    .PromoCodes
                    .FirstOrDefault()?
                    .Code;

            }
            catch (Exception ex)
            {
            }

            await context.RespondAsync(new SubscriptionGetPromoAndGiftResponse(
                PromoCode: promoCode,
                GiftId: subscription.GiftId
            ));
        }
    }
}
