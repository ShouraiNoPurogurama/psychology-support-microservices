using BuildingBlocks.DDD;

namespace Payment.Domain.Events;

public record PaymentDetailFailedEvent(
    Guid SubscriptionId,
    string PatientEmail,
    string? PromoCode,
    Guid? GiftId,
    decimal FinalPrice) : IDomainEvent;