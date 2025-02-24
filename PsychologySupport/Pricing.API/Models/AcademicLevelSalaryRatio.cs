using BuildingBlocks.DDD;

namespace Pricing.API.Models
{
    public enum AcademicLevel
    {
        Master,
        Doctor,
        Postdoctoral
    }

    public class AcademicLevelSalaryRatio : Entity<Guid>
    {
        public AcademicLevel AcademicLevel { get; set; }

        public decimal FeeMultiplier { get; set; }

    }
}
