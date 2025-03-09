namespace BuildingBlocks.Messaging.Events.Subscription;

public record ActivateSubscriptionIntegrationEvent(Guid SubscriptionId) : IntegrationEvents;