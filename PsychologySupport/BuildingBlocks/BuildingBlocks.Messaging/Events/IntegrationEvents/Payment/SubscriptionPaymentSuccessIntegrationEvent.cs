namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;

public record SubscriptionPaymentSuccessIntegrationEvent(Guid SubjectRef,Guid SubscriptionId) : IntegrationEvent;