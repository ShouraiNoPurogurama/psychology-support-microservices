using BuildingBlocks.Enums;

namespace Payment.Application.Payments.Dtos;

public record BasePaymentDto(decimal FinalPrice, Guid PatientId, PaymentMethodName PaymentMethod);