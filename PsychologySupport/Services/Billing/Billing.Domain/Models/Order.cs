using Billing.Domain.Enums;

namespace Billing.Domain.Models;

public partial class Order : AggregateRoot<Guid>
{
    public long OrderCode { get; private set; }
    public Guid SubjectRef { get; private set; }
    public string OrderType { get; private set; } = null!;
    public string ProductCode { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? PromoCode { get; private set; }
    public OrderStatus Status { get; private set; }
    public Guid? PaymentId { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public virtual ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

    private Order() { } 

    private Order(
        Guid id,
        long orderCode,
        Guid subjectRef,
        string orderType,
        string productCode,
        decimal amount,
        string currency,
        string? promoCode,
        OrderStatus status,
        string createdBy,
        string lastModifiedBy)
    {
        Id = id;
        OrderCode = orderCode;
        SubjectRef = subjectRef;
        OrderType = orderType;
        ProductCode = productCode;
        Amount = amount;
        Currency = currency;
        PromoCode = promoCode;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        LastModified = DateTime.UtcNow;
        LastModifiedBy = lastModifiedBy;
    }

    public static Order Create(
        long OrderCode, 
        Guid subjectRef,
        string orderType,
        string productCode,
        decimal amount,
        string currency,
        string? promoCode,
        string createdBy)
    {
        // Validations
        if (subjectRef == Guid.Empty)
            throw new InvalidDataException("SubjectRef không được để trống.");

        if (string.IsNullOrWhiteSpace(orderType))
            throw new InvalidDataException("OrderType không được để trống.");

        if (string.IsNullOrWhiteSpace(productCode))
            throw new InvalidDataException("ProductCode không được để trống.");

        if (amount < 0)
            throw new InvalidDataException("Số tiền không thể âm.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidDataException("Currency không được để trống.");

        if (!new[] { "BuySubscription", "BuyPoint" }.Contains(orderType))
            throw new InvalidDataException("OrderType không hợp lệ. Chỉ chấp nhận 'BuySubscription' hoặc 'BuyPoint'.");

        return new Order(
            id: Guid.NewGuid(),
            orderCode: OrderCode,
            subjectRef: subjectRef,
            orderType: orderType,
            productCode: productCode,
            amount: amount,
            currency: currency,
            promoCode: promoCode,
            status: OrderStatus.Pending,
            createdBy: createdBy,
            lastModifiedBy: createdBy
        );
    }
}