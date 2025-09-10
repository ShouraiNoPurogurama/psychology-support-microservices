using BuildingBlocks.DDD;
using BuildingBlocks.Exceptions;
using System;
using System.Collections.Generic;

namespace Billing.API.Models;

public partial class Order : AggregateRoot<Guid>
{
    public Guid SubjectRef { get; private set; }
    public string OrderType { get; private set; } = null!;
    public string ProductCode { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? PromoCode { get; private set; }
    public string Status { get; set; } = null!;
    public Guid? PaymentId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? IdempotencyKeyId { get; private set; }

    public virtual IdempotencyKey? IdempotencyKey { get; set; }
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    private Order() { } 

    private Order(
        Guid id,
        Guid subject_ref,
        string orderType,
        string productCode,
        decimal amount,
        string currency,
        string? promoCode,
        string status,
        Guid? idempotencyKeyId,
        string createdBy,
        string lastModifiedBy)
    {
        Id = id;
        SubjectRef = subject_ref;
        OrderType = orderType;
        ProductCode = productCode;
        Amount = amount;
        Currency = currency;
        PromoCode = promoCode;
        Status = status;
        IdempotencyKeyId = idempotencyKeyId;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        LastModified = DateTime.UtcNow;
        LastModifiedBy = lastModifiedBy;
    }

    public static Order Create(
        Guid subject_ref,
        string orderType,
        string productCode,
        decimal amount,
        string currency,
        string? promoCode,
        Guid? idempotencyKeyId,
        string createdBy)
    {
        // Validations

        if (subject_ref == Guid.Empty)
            throw new ArgumentException("SubjectRef is required.", nameof(subject_ref));

        if (string.IsNullOrWhiteSpace(orderType))
            throw new ArgumentException("OrderType is required.", nameof(orderType));

        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("ProductCode is required.", nameof(productCode));

        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        if (!new[] { "BuySubscription", "BuyPoint" }.Contains(orderType))
            throw new ArgumentException("Invalid OrderType. Must be 'BuySubscription' or 'BuyPoint'.", nameof(orderType));


        return new Order(
            id: Guid.NewGuid(),
            subject_ref: subject_ref,
            orderType: orderType,
            productCode: productCode,
            amount: amount,
            currency: currency,
            promoCode: promoCode,
            status: "AwaitPayment",
            idempotencyKeyId: idempotencyKeyId,
            createdBy: createdBy,
            lastModifiedBy: createdBy
        );
    }
}