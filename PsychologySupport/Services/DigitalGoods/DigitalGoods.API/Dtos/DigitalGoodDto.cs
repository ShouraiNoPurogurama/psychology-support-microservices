namespace DigitalGoods.API.Dtos
{
    public record DigitalGoodDto(
        Guid Id,
        string Name,
        string Type,
        string ConsumptionType,
        int Price,
        string? Description,
        string? MediaUrl,
        bool IsOwnedByUser
    );
}
