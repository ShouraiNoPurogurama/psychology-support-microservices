using BuildingBlocks.Messaging.Events.Scheduling;
using MassTransit;
using Promotion.Grpc;

namespace Scheduling.API.EventHandlers;

public class ValidateBookingRequestHandler(
    SchedulingDbContext dbContext,
    PromotionService.PromotionServiceClient promotionService) : IConsumer<ValidateBookingRequest>
{
    public async Task Consume(ConsumeContext<ValidateBookingRequest> context)
    {
        var request = context.Message;
        //Validate Schedule
        //Validate Promotion Code
        //Validate Gift
        //Validate Final Price

        GetPromotionByCodeResponse? promotion = null;
        GiftCodeActivateDto? appliedGift = null;
        List<string> errors = [];

        var booking = await dbContext.Bookings.FindAsync(request.BookingId);
        if (booking == null)
        {
            errors.Add("Booking not found");

            await context.RespondAsync(ValidateBookingResponse.Failed(errors));
            return;
        }

        if (request.PatientId != booking.PatientId)
        {
            errors.Add("PatientId not match");
        }

        if (request.DoctorId != booking.DoctorId)
        {
            errors.Add("DoctorId not match");
        }

        if (request.Date != booking.Date || request.StartTime != booking.StartTime)
        {
            errors.Add("Booking time does not match");
        }

        if (request.Duration != booking.Duration)
        {
            errors.Add("Booking duration does not match");
        }

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            promotion = await promotionService.GetPromotionByCodeAsync(new GetPromotionByCodeRequest
            {
                Code = request.PromoCode,
                IgnoreExpired = true
            });

            if (promotion.PromoCode is null)
            {
                errors.Add("Promotion code is not valid");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.GiftCodeId.ToString()))
        {
            var gifts = await promotionService.GetGiftCodeByPatientIdAsync(new GetGiftCodeByPatientIdRequest()
            {
                Id = request.PatientId.ToString()
            });

            appliedGift = gifts.GiftCode
                .FirstOrDefault(g => g.Id != request.GiftCodeId.ToString());

            if (appliedGift is null)
            {
                errors.Add("Gift code is not valid");
            }
        }

        var finalPrice = booking.Price
                         - (promotion?.PromoCode != null ? 0.01m * promotion.PromoCode.Value * booking.Price : 0)
                         - (appliedGift != null ? (decimal)appliedGift.MoneyValue : 0);

        if (finalPrice != request.FinalPrice)
        {
            errors.Add("Final price is not valid");
        }

        switch (errors.Count == 0)
        {
            case true:
                await context.RespondAsync(ValidateBookingResponse.Success());
                break;
            case false:
                await context.RespondAsync(ValidateBookingResponse.Failed(errors));
                break;
        }
    }
}