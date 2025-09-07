//using Billing.API.Data;
//using Billing.API.Domains.Billings.Dtos;
//using Billing.API.Domains.Billings.Models;
//using BuildingBlocks.CQRS;
//using BuildingBlocks.Exceptions;
//using BuildingBlocks.Messaging.Events.Queries.Billing;
//using BuildingBlocks.Messaging.Events.Queries.Payment;
//using BuildingBlocks.Messaging.Events.Queries.Wallet;
//using Mapster;
//using MassTransit;
//using Microsoft.EntityFrameworkCore;
//using Promotion.Grpc;
//using System.Data;
//using System.Reflection.Metadata;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.Json;

//namespace Billing.API.Domains.Billings.Features.CreateOrder;

//public record CreateOrderCommand(CreateOrderDto Dto) : ICommand<CreateOrderResult>;

//public record CreateOrderResult(Guid OrderId, string InvoiceCode, string PaymentUrl, long? PaymentCode);

//public class CreateOrderHandler(
//    BillingDbContext context,
//    IRequestClient<GetPointPackageRequest> pointPackageClient,
//    PromotionService.PromotionServiceClient promotionClient,
//    IRequestClient<GenerateOrderPaymentUrlRequest> paymentClient,
//    IRequestClient<GetPendingPaymentUrlForOrderRequest> paymentUrlClient
//) : ICommandHandler<CreateOrderCommand, CreateOrderResult>
//{
//    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
//    {
//        var dto = request.Dto ?? throw new BadRequestException("Empty request body.");
//        if (dto.Subject_ref == Guid.Empty)
//            throw new BadRequestException("Subject_ref is required.");

//        // Start serializable transaction to avoid race conditions
//        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

//        // Check subject_ref exists 


//        // Check for existing awaiting payment order
//        var awaitingPaymentOrder = await context.Orders
//            .FirstOrDefaultAsync(o => o.Subject_ref == dto.Subject_ref &&
//                                     o.ProductCode == dto.PointPackageCode &&
//                                     o.Status == "AwaitPayment", cancellationToken);

//        if (awaitingPaymentOrder != null)
//        {
//            var paymentUrlResponse = await paymentUrlClient.GetResponse<GetPendingPaymentUrlForOrderResponse>(
//                new GetPendingPaymentUrlForOrderRequest(awaitingPaymentOrder.Id), cancellationToken);

//            var existingPaymentUrl = paymentUrlResponse.Message.Url ??
//                                    throw new BadRequestException("Cannot retrieve payment URL for awaiting payment order.");
//            var existingPaymentCode = paymentUrlResponse.Message.PaymentCode;

//            return new CreateOrderResult(
//                awaitingPaymentOrder.Id,
//                awaitingPaymentOrder.Invoice?.Code ?? throw new BadRequestException("Invoice not found for awaiting payment order."),
//                existingPaymentUrl,
//                existingPaymentCode
//            );
//        }

//        // Step 2: Idempotency check
//        var requestJson = JsonSerializer.Serialize(dto);
//        var requestHash = ComputeSha256(requestJson);

//        var existingKey = await context.IdempotencyKeys
//            .FirstOrDefaultAsync(k => k.IdempotencyKey1 == dto.IdempotencyKey, cancellationToken);

//        if (existingKey != null)
//        {
//            if (!string.Equals(existingKey.RequestHash, requestHash, StringComparison.OrdinalIgnoreCase))
//                throw new BadRequestException("Idempotency key already used with different request payload.");

//            if (existingKey.ResponsePayload != null)
//            {
//                var replay = JsonSerializer.Deserialize<CreateOrderResult>(existingKey.ResponsePayload);
//                if (replay != null)
//                    return replay;
//            }

//            throw new ConflictException("Request with same idempotency key is in progress.");
//        }

//        var idKey = new IdempotencyKey
//        {
//            Id = Guid.NewGuid(),
//            IdempotencyKey = dto.IdempotencyKey,
//            RequestHash = requestHash,
//            ExpiresAt = DateTime.UtcNow.AddHours(24),
//            CreatedAt = DateTime.UtcNow,
//            //CreatedBy = dto.AliasId
//        };
//        context.IdempotencyKeys.Add(idKey);
//        await context.SaveChangesAsync(cancellationToken);

//        // Step 3: Get PointPackage from Wallet Service
//        var pkgResp = await pointPackageClient.GetResponse<GetPointPackageResponse>(
//            new GetPointPackageRequest(dto.PointPackageCode), cancellationToken);

//        var pkg = pkgResp.Message;
//        if (pkg == null)
//            throw new NotFoundException("PointPackage", dto.PointPackageCode);

//        decimal basePrice = pkg.Price;
//        string PackageName = pkg.Name;
//        string PackageCurrency = pkg.Currency;
//        string PackageDescription = pkg.Description ?? "Point Package Purchase";
//        decimal finalPrice = basePrice;
//        decimal totalDiscount = 0m;

//        // Step 4: Validate promo code and compute finalPrice
//        //if (!string.IsNullOrEmpty(dto.PromoCode))
//        //{
//        //    var promoResp = await promotionClient.ValidatePromoAsync(
//        //        new ValidatePromoRequest { Code = dto.PromoCode, UserId = dto.AliasId, OrderType = dto.OrderType },
//        //        cancellationToken: cancellationToken);

//        //    if (!promoResp.IsValid)
//        //        throw new BadRequestException("Invalid promo code.");

//        //    var discountResp = await promotionClient.CalculateDiscountAsync(
//        //        new CalculateDiscountRequest { BasePrice = basePrice, Code = dto.PromoCode },
//        //        cancellationToken: cancellationToken);

//        //    finalPrice = discountResp.DiscountedPrice;
//        //    totalDiscount = basePrice - finalPrice;
//        //    if (totalDiscount < 0) totalDiscount = 0;
//        //}

//        // Step 5: Create Order
//        var order = Order.Create(
//         subjectRef: dto.Subject_ref,
//         orderType: dto.OrderType,
//         productCode: dto.PackageCode,
//         amount: finalPrice,
//         currency: string.IsNullOrWhiteSpace(pkg.Currency) ? "VND" : pkg.Currency,
//         promoCode: dto.PromoCode,
//         idempotencyKeyId: idKey.Id
//         //createdBy: dto.AliasId
//        );
//        context.Orders.Add(order);

//        // Step 6: Create Invoice
//        var invoice = new Invoice
//        {
//            Id = Guid.NewGuid(),
//            Code = GenerateInvoiceCode(),
//            OrderId = order.Id,
//            SubjectRef = dto.AliasId,
//            Amount = finalPrice,
//            Status = "Issued",
//            IssuedAt = DateTime.UtcNow,
//            CreatedAt = DateTime.UtcNow,
//           //CreatedBy = dto.AliasId,
//            LastModified = DateTime.UtcNow,
//            //LastModifiedBy = dto.AliasId
//        };
//        context.Invoices.Add(invoice);

//        // Step 7: Create InvoiceSnapshot
//        var aliasInfoJson = JsonSerializer.Serialize(new { subject_ref = dto.AliasId.ToString() });
//        var snapshot = new InvoiceSnapshot
//        {
//            Id = Guid.NewGuid(),
//            InvoiceId = invoice.Id,
//            OrderType = dto.OrderType,
//            TotalDiscountAmount = totalDiscount,
//            AliasInfo = aliasInfoJson,
//            Currency = order.Currency,
//            TotalAmount = finalPrice,
//            TaxAmount = 0m,
//            CreatedAt = invoice.IssuedAt,
//            //CreatedBy = dto.AliasId,
//            LastModified = DateTime.UtcNow,
//           // LastModifiedBy = dto.AliasId
//        };
//        context.InvoiceSnapshots.Add(snapshot);

//        // Step 8: Create InvoiceItem
//        var item = new InvoiceItem
//        {
//            Id = Guid.NewGuid(),
//            InvoiceSnapshotId = snapshot.Id,
//            ItemType = "Point",
//            ProductCode = dto.PointPackageCode,
//            ProductName = pkg.Name,
//            PromoCode = dto.PromoCode,
//            Description = pkg.Description ?? "Point Package Purchase",
//            Quantity = 1,
//            Unit = "package",
//            UnitPrice = basePrice,
//            DiscountAmount = totalDiscount,
//            TotalAmount = finalPrice,
//            CreatedAt = DateTime.UtcNow,
//            //CreatedBy = dto.AliasId,
//            LastModified = DateTime.UtcNow,
//            //LastModifiedBy = dto.AliasId
//        };
//        context.InvoiceItems.Add(item);

//        // Step 9: Write Outbox messages
//        var orderCreatedPayload = new
//        {
//            order.Id,
//            order.SubjectRef,
//            order.OrderType,
//            order.Amount,
//            order.Currency,
//            order.PromoCode,
//            order.Status,
//            IdempotencyKeyId = idKey.Id,
//            order.ProductCode
//        };

//        var invoiceIssuedPayload = new
//        {
//            invoice.Id,
//            invoice.OrderId,
//            invoice.Code,
//            invoice.Amount,
//            invoice.Status,
//            invoice.IssuedAt
//        };

//        context.OutboxMessages.AddRange(
//            new OutboxMessage
//            {
//                Id = Guid.NewGuid(),
//                AggregateType = "Order",
//                AggregateId = order.Id,
//                EventType = "OrderCreated",
//                Payload = JsonDocument.Parse(JsonSerializer.Serialize(orderCreatedPayload)),
//                OccurredOn = DateTime.UtcNow
//            },
//            new OutboxMessage
//            {
//                Id = Guid.NewGuid(),
//                AggregateType = "Invoice",
//                AggregateId = invoice.Id,
//                EventType = "InvoiceIssued",
//                Payload = JsonDocument.Parse(JsonSerializer.Serialize(invoiceIssuedPayload)),
//                OccurredOn = DateTime.UtcNow
//            }
//        );

//        await context.SaveChangesAsync(cancellationToken);

    
//        var paymentReq = new GenerateOrderPaymentUrlRequest
//        (
//            OrderId: order.Id, 
//            Amount: order.Amount, 
//            Currency: order.Currency, 
//            PaymentMethodName: dto.PaymentMethodName, 
//            SubjectRef: dto.Subject_ref, 
//            PointPackageCode: dto.ProductCode
//        );

//        var paymentResp = await paymentClient.GetResponse<GenerateOrderPaymentUrlResponse>(paymentReq, cancellationToken);

//        if (paymentResp?.Message == null || string.IsNullOrEmpty(paymentResp.Message.Url))
//            throw new BadRequestException("Cannot create payment url.");

//        // Update Order

//        // Step 11: Update idempotency_keys.response_payload
//        var result = new CreateOrderResult(order.Id, invoice.Code, paymentResp.Message.Url, paymentResp.Message.PaymentCode);

//        idKey.ResponsePayload = JsonDocument.Parse(JsonSerializer.Serialize(result));
//        context.IdempotencyKeys.Update(idKey);
//        await context.SaveChangesAsync(cancellationToken);

//        await transaction.CommitAsync(cancellationToken);

//        return result;
//    }

//    private static string ComputeSha256(string raw)
//    {
//        using var sha = SHA256.Create();
//        var bytes = Encoding.UTF8.GetBytes(raw);
//        var hash = sha.ComputeHash(bytes);
//        var sb = new StringBuilder();
//        foreach (var b in hash) sb.Append(b.ToString("x2"));
//        return sb.ToString();
//    }

//    private static string GenerateInvoiceCode()
//    {
//        return $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}";
//    }
//}