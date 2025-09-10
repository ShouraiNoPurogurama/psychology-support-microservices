using Billing.API.Data;
using Billing.API.Domains.Billings.Dtos;
using Billing.API.Domains.Billings.Features.GetOrders;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Billing.API.Domains.Billings.Features.GetOrder;

public record GetOrderQuery(Guid OrderId) : IQuery<GetOrderResult>;


public record GetOrderResult(
    Guid OrderId,
    string OrderType,
    decimal Amount,
    string Currency,
    string Status,
    string? PromoCode,
    DateTimeOffset? CreatedAt,
    InvoiceDto Invoice
);

public class GetOrderHandler : IQueryHandler<GetOrderQuery, GetOrderResult>
{
    private readonly BillingDbContext _context;

    public GetOrderHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<GetOrderResult> Handle(GetOrderQuery query, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Invoices)
                .ThenInclude(i => i.InvoiceSnapshot)
                    .ThenInclude(s => s.InvoiceItems)
            .FirstOrDefaultAsync(o => o.Id == query.OrderId, cancellationToken);

        if (order == null)
            throw new NotFoundException("Order", query.OrderId);

        var invoice = order.Invoices.FirstOrDefault();
        if (invoice == null)
            throw new NotFoundException("Invoice", query.OrderId);

        var response = new GetOrderResult(
            order.Id,
            order.OrderType,
            order.Amount,
            order.Currency,
            order.Status,
            order.PromoCode,
            order.CreatedAt,
            new InvoiceDto(
                invoice.Id,
                invoice.Code,
                invoice.Status,
                invoice.InvoiceSnapshot.InvoiceItems?.Select(i =>
                    new InvoiceItemDto(
                        i.ItemType,
                        i.ProductCode,
                        i.ProductName,
                        i.Quantity,
                        i.UnitPrice,
                        i.TotalAmount
                    )
                ).ToList() ?? new List<InvoiceItemDto>()
            )
        );

        return response;
    }
}
