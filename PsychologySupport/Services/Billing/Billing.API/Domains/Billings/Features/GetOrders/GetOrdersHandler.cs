using Billing.API.Data;
using Billing.API.Domains.Billings.Dtos;
using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Billing.API.Domains.Billings.Features.GetOrders;

public record GetOrdersQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = "",                // search theo ProductCode || Status
    string? SortBy = "CreatedAt",       // field sort
    string? SortOrder = "desc",         // asc/desc
    string? OrderType = null,           // filter BuyPoint/BuySubscription
    string? Status = null               // filter Status
) : IQuery<GetOrdersResult>;

public record GetOrdersResult(PaginatedResult<OrderDto> Orders);

public class GetOrdersHandler : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    private readonly BillingDbContext _context;

    public GetOrdersHandler(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var pageIndex = Math.Max(0, request.PageIndex - 1);
        var pageSize = request.PageSize;

        var query = _context.Orders
            .Include(o => o.Invoices)
            .AsQueryable();

        // Filtering
        if (!string.IsNullOrEmpty(request.OrderType))
            query = query.Where(o => o.OrderType == request.OrderType);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(o => o.Status == request.Status);

        // Searching
        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(o =>
                o.ProductCode.Contains(request.Search) ||
                o.Status.Contains(request.Search));

        // Sorting
        query = (request.SortBy, request.SortOrder.ToLower()) switch
        {
            ("CreatedAt", "asc") => query.OrderBy(o => o.CreatedAt),
            ("CreatedAt", "desc") => query.OrderByDescending(o => o.CreatedAt),
            ("Amount", "asc") => query.OrderBy(o => o.Amount),
            ("Amount", "desc") => query.OrderByDescending(o => o.Amount),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDto(
                o.Id,
                o.OrderType,
                o.ProductCode,
                o.Amount,
                o.Currency,
                o.PromoCode,
                o.Status,
                o.Invoices.Select(i => i.Code).FirstOrDefault(),
                o.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new GetOrdersResult(new PaginatedResult<OrderDto>(
            pageIndex + 1,
            pageSize,
            totalCount,
            items
        ));
    }
}
