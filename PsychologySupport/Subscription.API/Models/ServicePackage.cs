using BuildingBlocks.DDD;

namespace Subscription.API.Models
{
        public class ServicePackage : Entity<Guid>
        {
            public string Name { get; set; } 

            public string Description { get; set; } 

            public decimal Price { get; set; }

            public int DurationDays { get; set; }
        }
 }

