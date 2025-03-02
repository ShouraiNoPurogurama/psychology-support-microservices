namespace Payment.Application.Dtos;

public record BasePaymentDto(decimal TotalAmount, Guid PatientId, PaymentMethod PaymentMethod);