using LifeStyles.API.Data.Common;

namespace LifeStyles.API.Dtos;

public record EntertainmentActivityDto(
    Guid Id,
    string Name,
    string Description,
    IntensityLevel IntensityLevel,
    ImpactLevel ImpactLevel
);