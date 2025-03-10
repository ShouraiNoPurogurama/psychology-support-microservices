using BuildingBlocks.DDD;

namespace Payment.Domain.Events;

public record SubscriptionPaymentCompletedEvent(
    Guid SubscriptionId,
    string PatientEmail,
    decimal FinalPrice) : IDomainEvent;