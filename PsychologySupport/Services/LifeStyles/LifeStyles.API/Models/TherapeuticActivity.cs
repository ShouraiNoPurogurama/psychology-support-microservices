using BuildingBlocks.Enums;

namespace LifeStyles.API.Models;

public class TherapeuticActivity
{
    public Guid Id { get; set; }
    public Guid TherapeuticTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public IntensityLevel IntensityLevel { get; set; }
    public ImpactLevel ImpactLevel { get; set; }
    public TherapeuticType TherapeuticType { get; set; }
}