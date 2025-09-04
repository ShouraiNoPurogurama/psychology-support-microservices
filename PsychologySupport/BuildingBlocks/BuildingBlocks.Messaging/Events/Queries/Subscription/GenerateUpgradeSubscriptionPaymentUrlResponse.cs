namespace BuildingBlocks.Messaging.Events.Queries.Subscription;

public record GenerateUpgradeSubscriptionPaymentUrlResponse(long? PaymentCode,string Url);