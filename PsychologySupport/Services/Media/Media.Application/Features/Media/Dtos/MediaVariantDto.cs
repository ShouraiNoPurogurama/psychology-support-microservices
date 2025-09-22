using Media.Domain.Enums;

namespace Media.Application.Features.Media.Dtos
{
    public record MediaVariantDto(
       Guid VariantId,
       VariantType VariantType,
       MediaFormat Format,
       int? Width,
       int? Height,
       string? CdnUrl
   );
}
