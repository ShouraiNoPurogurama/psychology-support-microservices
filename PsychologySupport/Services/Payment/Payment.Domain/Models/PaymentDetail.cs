using Payment.Domain.Enums;

namespace Payment.Domain.Models;

public class PaymentDetail
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }

    public string? ExternalTransactionCode { get; set; }
    public PaymentDetailStatus Status { get; set; } = PaymentDetailStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private PaymentDetail(decimal amount, string? externalTransactionCode)
    {
        Amount = amount;
        ExternalTransactionCode = externalTransactionCode;
    }

    public static PaymentDetail Of(decimal amount, string? externalTransactionCode = null)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        if (!string.IsNullOrWhiteSpace(externalTransactionCode) && externalTransactionCode.Length > 50)
            throw new ArgumentException("ExternalTransactionCode must be 50 characters or less.",
                nameof(externalTransactionCode));
        
        return new PaymentDetail(amount, externalTransactionCode);
    }

    public PaymentDetail MarkAsSuccess()
    {
        Status = PaymentDetailStatus.Completed;

        return this;
    }
    
    public PaymentDetail MarkAsFailed()
    {
        Status = PaymentDetailStatus.Failed;
        
        return this;
    }
}