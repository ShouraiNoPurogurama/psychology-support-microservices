namespace BuildingBlocks.Messaging.Events.Queries.Payment;

public record GetPendingPaymentUrlForSubscriptionResponse(long? PaymentCode,string? Url);