using Media.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Application.Dtos
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
