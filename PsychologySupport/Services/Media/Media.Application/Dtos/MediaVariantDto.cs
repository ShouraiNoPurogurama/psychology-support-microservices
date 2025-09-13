using Media.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Application.Dtos
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
