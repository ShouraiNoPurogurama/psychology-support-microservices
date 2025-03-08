using BuildingBlocks.Enums;

namespace LifeStyles.API.Models;

public class TherapeuticActivity
{
    public Guid Id { get; set; }
    public Guid TherapeuticTypeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Instructions { get; set; }
    public IntensityLevel IntensityLevel { get; set; }
    public ImpactLevel ImpactLevel { get; set; }

    public TherapeuticType TherapeuticType { get; set; }
}