using Payment.Domain.Models;

namespace Payment.Application.Dtos;

public record BasePaymentDto(decimal TotalAmount, Guid PatientId, PaymentMethod PaymentMethod);