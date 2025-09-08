using BuildingBlocks.Enums;

namespace Billing.API.Domains.Billings.Dtos
{
    public record CreateOrderDto(
        Guid Subject_ref,
        string ProductCode,
        string? PromoCode,
        PaymentMethodName PaymentMethodName,
        string OrderType // e.g., "BuyPoint"
    );
}
