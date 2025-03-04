using BuildingBlocks.Enums;

namespace Payment.Application.Payments.Dtos;

public record BasePaymentDto(decimal TotalAmount, Guid PatientId, PaymentMethodName PaymentMethod);