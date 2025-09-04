﻿using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Scheduling;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using BuildingBlocks.Messaging.Events.Queries.Scheduling;
using BuildingBlocks.Utils;
using Mapster;
using MassTransit;
using Promotion.Grpc;
using Scheduling.API.Dtos;
using Scheduling.API.Enums;
using Scheduling.API.Models;

namespace Scheduling.API.Features.CreateBooking
{
    public record CreateBookingCommand(CreateBookingDto BookingDto) : ICommand<CreateBookingResult>;

    public record CreateBookingResult(Guid BookingId, string BookingCode, string PaymentUrl);

    public class CreateBookingHandler(
        IRequestClient<GetPatientProfileRequest> patientClient,
        IRequestClient<GetDoctorProfileRequest> doctorClient,
        IRequestClient<GenerateBookingPaymentUrlRequest> paymentClient,
        PromotionService.PromotionServiceClient promotionService,
        SchedulingDbContext dbContext,
        IPublishEndpoint publisher
    )
        : ICommandHandler<CreateBookingCommand, CreateBookingResult>
    {
        public async Task<CreateBookingResult> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
        {
            var dto = command.BookingDto;
            // check patient
            var patient = await patientClient.GetResponse<GetPatientProfileResponse>(
                new GetPatientProfileRequest(dto.PatientId), cancellationToken);

            if (!patient.Message.PatientExists)
                throw new NotFoundException("Người dùng", dto.PatientId);

            // check doctor
            var doctor = await doctorClient.GetResponse<GetDoctorProfileResponse>(
                new GetDoctorProfileRequest(dto.DoctorId), cancellationToken);

            if (!doctor.Message.DoctorExists)
                throw new NotFoundException("Bác sĩ", dto.DoctorId);

            var isValidSlotDuration = await dbContext.DoctorSlotDurations
                .AnyAsync(d => d.DoctorId == dto.DoctorId && d.SlotDuration == dto.Duration, cancellationToken);

            if (!isValidSlotDuration)
                throw new BadRequestException("Thời lượng đặt lịch không khớp với thời lượng mà bác sĩ đã cấu hình.");

            // check booking time
            var bookingExistInSameTime = await dbContext.Bookings
                .AnyAsync(b => b.DoctorId == dto.DoctorId && b.Date == dto.Date && b.StartTime == dto.StartTime,
                    cancellationToken);

            if (bookingExistInSameTime)
            {
                throw new BadRequestException("Bác sĩ trong khung giờ này đã được người dùng khác đặt trước. Vui lòng chọn thời gian khác.");
            }

            //TODO Check price per hour of doctor matching with Experience and Pricing Service

            var (finalPrice, promoCodeDto) = await CalculateFinalPriceAsync(cancellationToken, dto);

            Guid.TryParse(promoCodeDto?.Code, out var promoCodeId);

            var bookingCode = CoreUtils.GenerateBookingCode(dto.Date);

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                BookingCode = bookingCode,
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                Duration = dto.Duration,
                Price = finalPrice,
                PromoCodeId = promoCodeId != Guid.Empty ? promoCodeId : null,
                GiftCodeId = dto.GiftCodeId,
                Status = BookingStatus.AwaitPayment
            };

            //Publish event to ChatBox
            var bookingCreatedIntegrationEvent = new BookingCreatedIntegrationEvent(
                doctor.Message.UserId,
                patient.Message.UserId,
                booking.Id
            );
            
            await publisher.Publish(bookingCreatedIntegrationEvent, cancellationToken);

            dbContext.Bookings.Add(booking);
            await dbContext.SaveChangesAsync(cancellationToken);

            #region Publish event to Payment

            var bookingCreatedEvent = dto.Adapt<GenerateBookingPaymentUrlRequest>();
            bookingCreatedEvent.BookingId = booking.Id;
            bookingCreatedEvent.PatientId = dto.PatientId;
            bookingCreatedEvent.PatientEmail = patient.Message.Email;
            bookingCreatedEvent.DoctorEmail = doctor.Message.Email;
            bookingCreatedEvent.FinalPrice = finalPrice;
            bookingCreatedEvent.PromoCode = promoCodeDto?.Code;
            bookingCreatedEvent.GiftId = dto.GiftCodeId;
            bookingCreatedEvent.PaymentType = PaymentType.Booking;

            var paymentUrl = await paymentClient.GetResponse<GenerateBookingPaymentUrlResponse>(
                bookingCreatedEvent.Adapt<GenerateBookingPaymentUrlRequest>(), cancellationToken);

            if (paymentUrl.Message is null)
                throw new BadRequestException("Cannot create payment url.");

            #endregion

            return new CreateBookingResult(booking.Id, bookingCode, paymentUrl.Message.Url);
        }

        private async Task<(decimal finalPrice, PromoCodeActivateDto? promotion)> CalculateFinalPriceAsync(
            CancellationToken cancellationToken,
            CreateBookingDto dto)
        {
            var finalPrice = dto.Price;

            if (string.IsNullOrEmpty(dto.PromoCode) && string.IsNullOrEmpty(dto.GiftCodeId.ToString()))
                return (finalPrice, null);

            var promotion = (await promotionService.GetPromotionByCodeAsync(new GetPromotionByCodeRequest
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
            if (dto.GiftCodeId is null) return (finalPrice, promotion);

            var giftCode = (await promotionService.GetGiftCodeByPatientIdAsync(new GetGiftCodeByPatientIdRequest()
                {
                    Id = dto.PatientId.ToString()
                }, cancellationToken: cancellationToken))
                .GiftCode
                .FirstOrDefault(g => g.Id == dto.GiftCodeId.ToString());

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
}