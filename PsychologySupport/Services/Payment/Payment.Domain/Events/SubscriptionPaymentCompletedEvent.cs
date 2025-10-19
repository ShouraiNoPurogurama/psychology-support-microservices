using BuildingBlocks.DDD;

namespace Payment.Domain.Events;

public record SubscriptionPaymentDetailCompletedEvent(
    Guid SubjectRef,
    Guid PatientId,
    Guid SubscriptionId,
    string PatientEmail,
    decimal FinalPrice) : IDomainEvent;