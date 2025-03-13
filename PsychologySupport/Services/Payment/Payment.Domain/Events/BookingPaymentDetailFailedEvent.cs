using BuildingBlocks.DDD;

namespace Payment.Domain.Events;

public record BookingPaymentDetailFailedEvent(
    Guid BookingId,
    string PatientEmail,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice) : IDomainEvent;