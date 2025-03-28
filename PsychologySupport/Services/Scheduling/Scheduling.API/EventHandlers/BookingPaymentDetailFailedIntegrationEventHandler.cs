using BuildingBlocks.Messaging.Events.Scheduling;
using MassTransit;
using MediatR;
using Promotion.Grpc;
using Scheduling.API.Enums;
using Scheduling.API.Features.UpdateBookingStatus;

namespace Scheduling.API.EventHandlers;

public class BookingPaymentDetailFailedIntegrationEventHandler(
    PromotionService.PromotionServiceClient promotionService,
    ISender sender) : IConsumer<BookingPaymentDetailFailedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<BookingPaymentDetailFailedIntegrationEvent> context)
    {
        //Cancel booking
        //Re-active promotions

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

        await sender.Send(new UpdateBookingStatusCommand(message.BookingId, BookingStatus.PaymentFailed));
    }
}