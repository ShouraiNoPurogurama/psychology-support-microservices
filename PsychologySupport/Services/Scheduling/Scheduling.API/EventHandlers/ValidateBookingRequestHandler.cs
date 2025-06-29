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
            errors.Add("Không tìm thấy lịch hẹn tương ứng.\n");

            await context.RespondAsync(ValidateBookingResponse.Failed(errors));
            return;
        }

        if (request.PatientId != booking.PatientId)
        {
            errors.Add("Tài khoản người dùng không khớp với lịch hẹn.");
        }

        if (request.DoctorId != booking.DoctorId)
        {
            errors.Add("Bác sĩ không đúng với lịch hẹn đã chọn.");
        }

        if (request.Date != booking.Date || request.StartTime != booking.StartTime)
        {
            errors.Add("Thời gian hẹn không trùng khớp với thông tin đặt lịch.");
        }

        if (request.Duration != booking.Duration)
        {
            errors.Add("Thời lượng khám không khớp với thông tin lịch hẹn.");
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
                errors.Add("Mã khuyến mãi không hợp lệ hoặc đã hết hạn.\n");
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
                errors.Add("Mã quà tặng không hợp lệ hoặc không thuộc về người dùng.");
            }
        }

        var finalPrice = booking.Price
                         - (promotion?.PromoCode != null ? 0.01m * promotion.PromoCode.Value * booking.Price : 0)
                         - (appliedGift != null ? (decimal)appliedGift.MoneyValue : 0);

        if (finalPrice != request.FinalPrice)
        {
            errors.Add("Giá cuối cùng không hợp lệ. Vui lòng kiểm tra lại khuyến mãi và mã quà tặng.");
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