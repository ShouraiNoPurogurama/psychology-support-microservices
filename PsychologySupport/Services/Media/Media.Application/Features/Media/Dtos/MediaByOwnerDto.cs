using Media.Domain.Enums;

namespace Media.Application.Features.Media.Dtos
{
    public record MediaByOwnerDto(
        Guid MediaId,
        MediaState State,
        List<MediaVariantByOwnerDto> Variants
    );

    public record MediaVariantByOwnerDto(
        VariantType VariantType,
        string CdnUrl
    );
}
