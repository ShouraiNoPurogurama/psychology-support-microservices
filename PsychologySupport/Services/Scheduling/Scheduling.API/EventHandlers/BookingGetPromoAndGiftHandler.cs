using BuildingBlocks.Messaging.Events.Scheduling;
using MassTransit;
using Promotion.Grpc;

namespace Scheduling.API.EventHandlers
{
    public class BookingGetPromoAndGiftHandler(
        SchedulingDbContext dbContext,
        PromotionService.PromotionServiceClient promotionService
    ) : IConsumer<BookingGetPromoAndGiftRequestEvent>
    {
        public async Task Consume(ConsumeContext<BookingGetPromoAndGiftRequestEvent> context)
        {
            var request = context.Message;

            var booking = await dbContext.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.BookingId);

            if (booking == null)
            {
                await context.RespondAsync(new BookingGetPromoAndGiftResponseEvent(null, null));
                return;
            }

            string? promoCode = null;

            try
            {
                var promoResponse = await promotionService.GetPromotionByIdAsync(new Promotion.Grpc.GetPromotionByIdRequest
                {
                    PromotionId = booking.PromoCodeId?.ToString() ?? Guid.Empty.ToString()
                });

                promoCode = promoResponse.Promotion
                    .PromoCodes
                    .FirstOrDefault()?
                    .Code;
            }
            catch (Exception)
            {
            }

            await context.RespondAsync(new BookingGetPromoAndGiftResponseEvent(
                PromoCode: promoCode,
                GiftId: booking.GiftCodeId
            ));
        }
    }
}
