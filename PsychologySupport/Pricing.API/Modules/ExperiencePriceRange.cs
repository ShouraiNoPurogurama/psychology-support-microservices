namespace Pricing.API.Modules
{
    public class ExperiencePriceRange
    {
        public Guid Id { get; set; } 

        public int MinYearsOfExperience { get; set; }

        public int MaxYearsOfExperience { get; set; }

        public decimal PricePerSession { get; set; }

        public decimal PricePerMinute { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; }
    }
}
