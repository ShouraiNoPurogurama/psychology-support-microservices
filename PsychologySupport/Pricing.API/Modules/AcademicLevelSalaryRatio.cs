namespace Pricing.API.Modules
{
    public enum AcademicLevel
    {
        Master,
        Doctor,
        Postdoctoral
    }

    public class AcademicLevelSalaryRatio
    {
        public Guid Id { get; set; } 

        public AcademicLevel AcademicLevel { get; set; }

        public decimal FeeMultiplier { get; set; }

        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; }
    }
}
