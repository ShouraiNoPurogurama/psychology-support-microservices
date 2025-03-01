using LifeStyles.API.Data.Common;

namespace LifeStyles.API.Dtos;

public record PhysicalActivityDto(
    Guid Id,
    string Name,
    string Description,
    IntensityLevel IntensityLevel,
    ImpactLevel ImpactLevel
);