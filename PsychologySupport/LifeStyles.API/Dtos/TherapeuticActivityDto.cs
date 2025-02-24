using LifeStyles.API.Data.Common;

namespace LifeStyles.API.Dtos
{
    public record TherapeuticActivityDto(
        Guid Id,
        string TherapeuticTypeName,
        string Name,
        string Description,
        string Instructions,
        string IntensityLevel,
        string ImpactLevel
    );
}
