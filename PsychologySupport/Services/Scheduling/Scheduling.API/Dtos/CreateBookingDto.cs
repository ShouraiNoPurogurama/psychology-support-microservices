using BuildingBlocks.Enums;

namespace Scheduling.API.Dtos;

public record CreateBookingDto(
    Guid DoctorId,
    Guid PatientId,
    DateOnly Date,
    TimeOnly StartTime,
    int Duration,
    decimal Price,
    string? PromoCode,
    Guid? GiftCodeId,
    PaymentMethodName PaymentMethod
);