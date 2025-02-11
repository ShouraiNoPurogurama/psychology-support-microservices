using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Subscription.API.Models
{
        public class ServicePackage
        {
            public Guid Id { get; set; }

            public string Name { get; set; } 

            public string Description { get; set; } 

            public decimal Price { get; set; }

            public int DurationDays { get; set; }

            public DateTime CreatedAt { get; set; }

            public DateTime UpdatedAt { get; set; }

            public string CreatedBy { get; set; } 
        }
 }

