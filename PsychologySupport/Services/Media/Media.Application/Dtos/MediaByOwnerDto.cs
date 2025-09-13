using Media.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Application.Dtos
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
