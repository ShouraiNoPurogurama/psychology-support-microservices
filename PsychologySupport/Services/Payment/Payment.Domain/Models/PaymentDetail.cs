using Payment.Domain.Enums;

namespace Payment.Domain.Models;

public class PaymentDetail
{
    public Guid Id { get; set; }
        
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
        
    public string? ExternalTransactionCode { get; set; }
    public PaymentDetailStatus Status { get; set; }
        
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual Payment Payment { get; set; }
}
