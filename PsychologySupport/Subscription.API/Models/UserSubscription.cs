using BuildingBlocks.DDD;
using Subscription.API.Data.Common;

namespace Subscription.API.Models
{
    public class UserSubscription : Entity<Guid>
    {
        public Guid PatientId { get; set; }

        public Guid ServicePackageId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public SubscriptionStatus Status { get; set; }

    }
}
