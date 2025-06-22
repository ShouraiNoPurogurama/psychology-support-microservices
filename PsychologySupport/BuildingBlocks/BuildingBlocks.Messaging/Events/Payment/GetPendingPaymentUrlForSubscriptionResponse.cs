namespace BuildingBlocks.Messaging.Events.Payment;

public record GetPendingPaymentUrlForSubscriptionResponse(long? PaymentCode,string? Url);