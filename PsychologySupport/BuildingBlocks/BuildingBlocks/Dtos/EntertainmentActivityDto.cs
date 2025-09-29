using BuildingBlocks.Enums;

namespace BuildingBlocks.Dtos
{
    public record EntertainmentActivityDto(
    Guid Id,
    string Name,
    string Description,
    IntensityLevel IntensityLevel,
    ImpactLevel ImpactLevel
    ) : IActivityDto; 
}
