using BuildingBlocks.Enums;

namespace Payment.Domain.Models;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public PaymentMethodName Name { get; set; }

    public string? Details { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}