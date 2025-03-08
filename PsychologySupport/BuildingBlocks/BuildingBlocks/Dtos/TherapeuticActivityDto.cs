using BuildingBlocks.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Dtos
{
    public record TherapeuticActivityDto(
    Guid Id,
    string TherapeuticTypeName,
    string Name,
    string Description,
    string Instructions,
    IntensityLevel IntensityLevel,
    ImpactLevel ImpactLevel
    ) : IActivityDto;
}
