using Billing.Domain.Enums;

namespace Billing.Domain.Models;

public partial class Invoice : AggregateRoot<Guid>
{
    public string Code { get; private set; } = null!;
    public Guid OrderId { get; private set; }
    public Guid SubjectRef { get; private set; }
    public decimal Amount { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset IssuedAt { get; private set; }

    public virtual InvoiceSnapshot? InvoiceSnapshot { get; private set; }
    public virtual Order Order { get; private set; } = null!;

    private Invoice() { }

    private Invoice(
        Guid id,
        string code,
        Guid orderId,
        Guid subjectRef,
        decimal amount,
        InvoiceStatus status,
        DateTimeOffset issuedAt,
        string createdBy)
    {
        Id = id;
        Code = code;
        OrderId = orderId;
        SubjectRef = subjectRef;
        Amount = amount;
        Status = status;
        IssuedAt = issuedAt;

        CreatedAt = DateTimeOffset.UtcNow;
        CreatedBy = createdBy;
        LastModified = DateTimeOffset.UtcNow;
        LastModifiedBy = createdBy;
    }

    public static Invoice Create(
        string code,
        Guid orderId,
        Guid subjectRef,
        decimal amount,
        string createdBy)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidDataException("Mã hóa đơn (Code) không được để trống.");

        if (orderId == Guid.Empty)
            throw new InvalidDataException("OrderId không được để trống.");

        if (subjectRef == Guid.Empty)
            throw new InvalidDataException("SubjectRef không được để trống.");

        if (amount <= 0)
            throw new InvalidDataException("Số tiền (Amount) phải lớn hơn 0.");

        return new Invoice(
            id: Guid.NewGuid(),
            code: code,
            orderId: orderId,
            subjectRef: subjectRef,
            amount: amount,
            status: InvoiceStatus.Issued,
            issuedAt: DateTimeOffset.UtcNow,
            createdBy: createdBy
        );
    }

    public void MarkAsPaid(string modifiedBy)
    {
        if (Status != InvoiceStatus.Issued)
            throw new InvalidOperationException("Chỉ hóa đơn ở trạng thái 'Issued' mới có thể thanh toán.");

        Status = InvoiceStatus.Paid;
        LastModified = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Cancel(string modifiedBy)
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Không thể hủy hóa đơn đã thanh toán.");

        Status = InvoiceStatus.Cancelled;
        LastModified = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}
