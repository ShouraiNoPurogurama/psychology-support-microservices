using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Subscription.API.Models
{
    public enum SubscriptionStatus
    {
        Active,
        Expired,
        Cancelled
    }
    public class UserSubscription
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ServicePackageId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        public SubscriptionStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; } = default!;
    }
}
