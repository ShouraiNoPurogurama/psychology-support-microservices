namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;

public record SubscriptionPaymentSuccessIntegrationEvent(Guid SubscriptionId) : IntegrationEvent;