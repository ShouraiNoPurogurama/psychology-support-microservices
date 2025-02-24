namespace LifeStyles.API.Dtos
{
    public record EntertainmentActivityDto(
        Guid Id,
        string Name,
        string Description,
        string IntensityLevel,
        string ImpactLevel
    );
}
