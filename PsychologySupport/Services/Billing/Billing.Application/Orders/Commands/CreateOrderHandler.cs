using Billing.Application.Data;
using Billing.Application.Dtos;
using Billing.Domain.Enums;
using Billing.Domain.Models;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Queries.Billing;
using BuildingBlocks.Messaging.Events.Queries.Payment;
using BuildingBlocks.Messaging.Events.Queries.Wallet;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Promotion.Grpc;
using System.Data;
using System.Text.Json;

namespace Billing.API.Domains.Billings.Features.CreateOrder;

public record CreateOrderCommand(Guid RequestKey, CreateOrderDto Dto)
    : IdempotentCommand<CreateOrderResult>(RequestKey);

public record CreateOrderResult(Guid OrderId, string InvoiceCode, string PaymentUrl, long? PaymentCode);

public class CreateOrderHandler(
    IBillingDbContext context,
    IRequestClient<GetPointPackageRequest> pointPackageClient,
    PromotionService.PromotionServiceClient promotionClient,
    IRequestClient<GenerateOrderPaymentUrlRequest> paymentClient,
    IRequestClient<GetPendingPaymentUrlForOrderRequest> paymentUrlClient
    //IIdempotencyService idempotencyService
) : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IBillingDbContext _context = context;
    private readonly IRequestClient<GetPointPackageRequest> _pointPackageClient = pointPackageClient;
    private readonly PromotionService.PromotionServiceClient _promotionClient = promotionClient;
    private readonly IRequestClient<GenerateOrderPaymentUrlRequest> _paymentClient = paymentClient;
    private readonly IRequestClient<GetPendingPaymentUrlForOrderRequest> _paymentUrlClient = paymentUrlClient;
    //private readonly IIdempotencyService _idempotencyService = idempotencyService;

    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto ?? throw new BadRequestException("Empty request body.");
        if (dto.Subject_ref == Guid.Empty)
            throw new BadRequestException("Subject_ref is required.");

        // Start serializable transaction to avoid race conditions
        await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        // Check for existing awaiting payment order
        var awaitingPaymentOrder = await _context.Orders
            .FirstOrDefaultAsync(o => o.SubjectRef == dto.Subject_ref &&
                                     o.ProductCode == dto.ProductCode &&
                                     o.Status == OrderStatus.Pending, cancellationToken);

        if (awaitingPaymentOrder != null)
        {
            var paymentUrlResponse = await _paymentUrlClient.GetResponse<GetPendingPaymentUrlForOrderResponse>(
                new GetPendingPaymentUrlForOrderRequest(awaitingPaymentOrder.Id), cancellationToken);

            var existingPaymentUrl = paymentUrlResponse.Message.Url ??
                                    throw new BadRequestException("Cannot retrieve payment URL for awaiting payment order.");
            var existingPaymentCode = paymentUrlResponse.Message.PaymentCode;

            return new CreateOrderResult(
                awaitingPaymentOrder.Id,
                awaitingPaymentOrder.Invoices.FirstOrDefault()?.Code ?? throw new BadRequestException("Invoice not found for awaiting payment order."),
                existingPaymentUrl,
                existingPaymentCode
            );
        }

        // Idempotency check

        // Create new IdempotencyKey


        // Get PointPackage from Wallet Service
        var pkgResp = await _pointPackageClient.GetResponse<GetPointPackageResponse>(
            new GetPointPackageRequest(dto.ProductCode), cancellationToken);

        var pkg = pkgResp.Message;
        if (pkg == null)
            throw new NotFoundException("PointPackage", dto.ProductCode);

        decimal basePrice = pkg.Price;
        string PackageName = pkg.Name;
        string PackageCurrency = pkg.Currency;
        string PackageDescription = pkg.Description;
        decimal finalPrice = basePrice;
        decimal totalDiscount = 0m;

        // Validate promo code and compute finalPrice 
        //if (!string.IsNullOrEmpty(dto.PromoCode))
        //{
        //    var promoResp = await _promotionClient.ValidatePromoAsync(
        //        new ValidatePromoRequest { Code = dto.PromoCode, UserId = dto.AliasId, OrderType = dto.OrderType },
        //        cancellationToken: cancellationToken);
        //
        //    if (!promoResp.IsValid)
        //        throw new BadRequestException("Invalid promo code.");
        //
        //    var discountResp = await _promotionClient.CalculateDiscountAsync(
        //        new CalculateDiscountRequest { BasePrice = basePrice, Code = dto.PromoCode },
        //        cancellationToken: cancellationToken);
        //
        //    finalPrice = discountResp.DiscountedPrice;
        //    totalDiscount = basePrice - finalPrice;
        //    if (totalDiscount < 0) totalDiscount = 0;
        //}

        // Create Order
        var order = Order.Create(
            subjectRef: dto.Subject_ref,
            orderType: dto.OrderType,
            productCode: dto.ProductCode,
            amount: finalPrice,
            currency: pkg.Currency,
            promoCode: dto.PromoCode,
            createdBy: "None"
        );
        _context.Orders.Add(order);

        // Create Invoice
        var invoice = Invoice.Create(
            code: GenerateInvoiceCode(),
            orderId: order.Id,
            subjectRef: dto.Subject_ref,
            amount: finalPrice,
            createdBy: "CT" //  lấy alias_id từ subjectRef
        );
        _context.Invoices.Add(invoice);

        // Create InvoiceSnapshot
        var aliasInfoJson = JsonSerializer.Serialize(new { subject_ref = dto.Subject_ref.ToString() }); // Replace with PiiService
        var snapshot = new InvoiceSnapshot
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            OrderType = dto.OrderType,
            TotalDiscountAmount = totalDiscount,
            AliasInfo = aliasInfoJson,
            Currency = order.Currency,
            TotalAmount = finalPrice,
            TaxAmount = 0m,
            CreatedAt = invoice.IssuedAt,
            LastModified = DateTime.UtcNow
            // CreatedBy = dto.AliasId, // Replace with PiiService if needed
            // LastModifiedBy = dto.AliasId
        };
        _context.InvoiceSnapshots.Add(snapshot);

        // Create InvoiceItem
        var item = new InvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceSnapshotId = snapshot.Id,
            ItemType = "Point",
            ProductCode = dto.ProductCode,
            ProductName = pkg.Name,
            PromoCode = dto.PromoCode,
            Description = pkg.Description,
            Quantity = 1,
            Unit = "package",
            UnitPrice = basePrice,
            DiscountAmount = totalDiscount,
            TotalAmount = finalPrice,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
            // CreatedBy = dto.AliasId, 
            // LastModifiedBy = dto.AliasId
        };
        _context.InvoiceItems.Add(item);

        // Write Outbox messages
        var orderCreatedPayload = new
        {
            order.Id,
            order.SubjectRef,
            order.OrderType,
            order.Amount,
            order.Currency,
            order.PromoCode,
            order.Status,
            order.ProductCode
        };

        var invoiceIssuedPayload = new
        {
            invoice.Id,
            invoice.OrderId,
            invoice.Code,
            invoice.Amount,
            invoice.Status,
            invoice.IssuedAt
        };

        _context.OutboxMessages.AddRange(
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                AggregateType = "Order",
                AggregateId = order.Id,
                EventType = "OrderCreated",
                Payload = JsonSerializer.Serialize(orderCreatedPayload),
                OccurredOn = DateTime.UtcNow
            },
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                AggregateType = "Invoice",
                AggregateId = invoice.Id,
                EventType = "InvoiceIssued",
                Payload = JsonSerializer.Serialize(invoiceIssuedPayload),
                OccurredOn = DateTime.UtcNow
            }
        );

        await _context.SaveChangesAsync(cancellationToken);

        // Generate Payment URL
        var paymentReq = new GenerateOrderPaymentUrlRequest
        (
            OrderId: order.Id,
            Amount: order.Amount,
            Currency: order.Currency,
            PaymentMethodName: dto.PaymentMethodName,
            SubjectRef: dto.Subject_ref,
            PointPackageCode: dto.ProductCode
        );

        var paymentResp = await _paymentClient.GetResponse<GenerateOrderPaymentUrlResponse>(paymentReq, cancellationToken);

        if (paymentResp?.Message == null || string.IsNullOrEmpty(paymentResp.Message.Url))
            throw new BadRequestException("Cannot create payment URL: Invalid or empty response from payment service.");

        // Update Order Status

        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);

        // Create result
        var result = new CreateOrderResult(order.Id, invoice.Code, paymentResp.Message.Url, paymentResp.Message.PaymentCode);

        // Save response to Idempotency storage

        await transaction.CommitAsync(cancellationToken);

        return result;
    }

    private static string GenerateInvoiceCode()
    {
        return $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}";
    }
}
