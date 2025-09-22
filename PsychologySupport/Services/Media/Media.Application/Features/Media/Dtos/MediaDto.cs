using Media.Domain.Enums;

namespace Media.Application.Features.Media.Dtos
{
    public record MediaDto(
        Guid MediaId,
        MediaState State,
        string SourceMime,
        long SourceBytes,
        string ChecksumSha256,
        List<MediaVariantDto> Variants
    );
}
