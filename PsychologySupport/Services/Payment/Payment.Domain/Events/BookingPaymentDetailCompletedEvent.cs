using BuildingBlocks.DDD;

namespace Payment.Domain.Events;

public record BookingPaymentDetailCompletedEvent(
    Guid BookingId,
    string PatientEmail,
    decimal FinalPrice) : IDomainEvent;