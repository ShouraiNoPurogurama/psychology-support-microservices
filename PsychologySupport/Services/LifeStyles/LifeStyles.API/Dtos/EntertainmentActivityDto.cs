using BuildingBlocks.Enums;

namespace LifeStyles.API.Dtos;

public record EntertainmentActivityDto(
    Guid Id,
    string Name,
    string Description,
    IntensityLevel IntensityLevel,
    ImpactLevel ImpactLevel
);