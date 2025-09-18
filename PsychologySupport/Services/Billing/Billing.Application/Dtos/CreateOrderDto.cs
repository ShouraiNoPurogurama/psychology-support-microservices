using BuildingBlocks.Enums;

namespace Billing.Application.Dtos
{
    public record CreateOrderDto(
        Guid Subject_ref,
        string ProductCode,
        string? PromoCode,
        PaymentMethodName PaymentMethodName,
        string OrderType // e.g., "BuyPoint"
    );
}
