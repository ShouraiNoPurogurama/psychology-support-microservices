using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Application.Features.ModuleSections.Dtos
{
    public record WellnessModuleDto(
     Guid Id,
     string Name,
     string MediaUrl,
     string? Description,
     int SectionCount
 );
}
