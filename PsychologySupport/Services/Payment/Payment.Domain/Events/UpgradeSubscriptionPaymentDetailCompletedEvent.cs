using BuildingBlocks.DDD;
using MediatR;

namespace Payment.Domain.Events;

public record UpgradeSubscriptionPaymentDetailCompletedEvent(
    Guid SubjectRef,
    Guid SubscriptionId,
    string PatientEmail,
    decimal FinalPrice) : IDomainEvent;