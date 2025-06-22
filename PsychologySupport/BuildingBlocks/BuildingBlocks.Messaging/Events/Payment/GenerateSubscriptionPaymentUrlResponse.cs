namespace BuildingBlocks.Messaging.Events.Payment;

public record GenerateSubscriptionPaymentUrlResponse(long? PaymentCode,string Url);