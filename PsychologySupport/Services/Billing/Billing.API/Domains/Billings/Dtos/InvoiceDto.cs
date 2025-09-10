using Billing.API.Data.Common;

namespace Billing.API.Domains.Billings.Dtos
{
    public record InvoiceDto(
    Guid InvoiceId,
    string Code,
    InvoiceStatus Status,
    List<InvoiceItemDto> Items
);
}
