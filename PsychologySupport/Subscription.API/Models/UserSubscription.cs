using BuildingBlocks.DDD;

namespace Subscription.API.Models
{
    public enum SubscriptionStatus
    {
        Active,
        Expired,
        Cancelled
    }
    public class UserSubscription : Entity<Guid>
    {
        public Guid UserId { get; set; }

        public Guid ServicePackageId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public SubscriptionStatus Status { get; set; }
    }
}
