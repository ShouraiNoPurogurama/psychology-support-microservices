namespace Billing.API.Domains.Billings.Dtos
{

    public record OrderDto(
            Guid Id,
            string OrderType,
            string ProductCode,
            decimal Amount,
            string Currency,
            string? PromoCode,
            string Status,
            string InvoiceCode,
            DateTimeOffset? CreatedAt
        );

}
