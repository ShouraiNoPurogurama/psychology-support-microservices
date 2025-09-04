namespace BuildingBlocks.Messaging.Events.Queries.Payment;

public record GetPendingPaymentUrlForSubscriptionRequest(Guid SubscriptionId);