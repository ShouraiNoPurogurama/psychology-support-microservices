namespace Payment.Domain.Models;

public class PaymentMethod
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
        
    public string? Details { get; set; }
    
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}