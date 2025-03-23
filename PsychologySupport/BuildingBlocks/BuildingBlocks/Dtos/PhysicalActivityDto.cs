using BuildingBlocks.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Dtos
{
    public record PhysicalActivityDto(
    Guid Id,
    string Name,
    string Description,
    IntensityLevel IntensityLevel,
    string ImpactLevel
    ) : IActivityDto;
}
