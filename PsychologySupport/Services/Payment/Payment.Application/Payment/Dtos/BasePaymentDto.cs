using BuildingBlocks.Enums;

namespace Payment.Application.Payment.Dtos;

public record BasePaymentDto(decimal TotalAmount, Guid PatientId, PaymentMethodName PaymentMethod);