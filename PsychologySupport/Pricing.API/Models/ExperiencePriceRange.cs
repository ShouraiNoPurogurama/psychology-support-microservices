using BuildingBlocks.DDD;

namespace Pricing.API.Models
{
    public class ExperiencePriceRange : Entity<Guid>
    {
        public int MinYearsOfExperience { get; set; }

        public int MaxYearsOfExperience { get; set; }

        public decimal PricePerSession { get; set; }

        public decimal PricePerMinute { get; set; }
    }
}
