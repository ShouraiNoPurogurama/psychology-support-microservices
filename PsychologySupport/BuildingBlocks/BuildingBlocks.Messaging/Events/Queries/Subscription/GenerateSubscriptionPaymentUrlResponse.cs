namespace BuildingBlocks.Messaging.Events.Queries.Subscription;

public record GenerateSubscriptionPaymentUrlResponse(long? PaymentCode,string Url);