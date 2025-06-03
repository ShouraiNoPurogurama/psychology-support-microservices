using BuildingBlocks.Enums;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Dtos;

public record PaymentDto(
    Guid Id,
    Guid PatientProfileId,
    decimal TotalAmount,
    Guid? SubscriptionId,
    Guid? BookingId,
    PaymentStatus Status,
    PaymentType PaymentType,
    Guid PaymentMethodId,
    string? ExternalTransactionCode,
    DateTimeOffset CreatedAt
);