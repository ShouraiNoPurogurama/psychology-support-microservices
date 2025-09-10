namespace Billing.API.Domains.Billings.Dtos
{
    public record InvoiceDto(
    Guid InvoiceId,
    string Code,
    string Status,
    List<InvoiceItemDto> Items
);
}
