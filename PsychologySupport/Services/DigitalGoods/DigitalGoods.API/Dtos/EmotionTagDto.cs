namespace DigitalGoods.API.Dtos
{
    public record EmotionTagDto(
         Guid Id,
         string Code,
         string DisplayName,
         string? UnicodeCodepoint,
         string? Topic,
         int SortOrder,
         bool IsActive,
         string Scope,
         Guid? MediaId
     );
}
