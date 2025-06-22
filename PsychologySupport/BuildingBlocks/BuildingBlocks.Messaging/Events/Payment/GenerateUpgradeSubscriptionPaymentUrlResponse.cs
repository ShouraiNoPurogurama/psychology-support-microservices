namespace BuildingBlocks.Messaging.Events.Payment;

public record GenerateUpgradeSubscriptionPaymentUrlResponse(long? PaymentCode,string Url);