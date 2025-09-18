using Billing.Domain.Enums;

namespace Billing.Application.Dtos
{
    public record InvoiceDto(
    Guid InvoiceId,
    string Code,
    InvoiceStatus Status,
    List<InvoiceItemDto> Items
);
}
