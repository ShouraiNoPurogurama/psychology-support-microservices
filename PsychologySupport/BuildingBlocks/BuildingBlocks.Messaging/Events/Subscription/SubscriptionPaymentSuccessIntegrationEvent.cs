namespace BuildingBlocks.Messaging.Events.Subscription;

public record SubscriptionPaymentSuccessIntegrationEvent(Guid SubscriptionId) : IntegrationEvents;