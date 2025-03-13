using BuildingBlocks.Enums;

namespace Payment.Application.Payments.Dtos;

public record BuyBookingDto(
    Guid BookingId,
    Guid DoctorId,
    Guid PatientId,
    string PatientEmail,
    DateOnly Date,
    TimeOnly StartTime,
    int Duration,
    decimal FinalPrice,
    string? PromoCode,
    Guid? GiftCodeId,
    PaymentMethodName PaymentMethod,
    PaymentType PaymentType) : BasePaymentDto(FinalPrice, PatientId, PaymentMethod);