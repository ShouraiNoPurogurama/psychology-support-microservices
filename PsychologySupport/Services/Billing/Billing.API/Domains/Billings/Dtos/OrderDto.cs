using Billing.API.Data.Common;

namespace Billing.API.Domains.Billings.Dtos
{

    public record OrderDto(
            Guid Id,
            string OrderType,
            string ProductCode,
            decimal Amount,
            string Currency,
            string? PromoCode,
            OrderStatus Status,
            string InvoiceCode,
            DateTimeOffset? CreatedAt
        );

}
