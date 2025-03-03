using LifeStyles.API.Data.Common;

namespace LifeStyles.API.Models;

public class PhysicalActivity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public IntensityLevel IntensityLevel { get; set; }
    public ImpactLevel ImpactLevel { get; set; }
}