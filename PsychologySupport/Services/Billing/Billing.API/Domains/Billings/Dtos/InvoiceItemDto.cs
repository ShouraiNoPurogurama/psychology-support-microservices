namespace Billing.API.Domains.Billings.Dtos
{
    public record InvoiceItemDto(
    string ItemType,
    string ProductCode,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalAmount
);
}
