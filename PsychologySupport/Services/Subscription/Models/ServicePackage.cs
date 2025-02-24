using BuildingBlocks.DDD;

namespace Subscription.API.Models
{
    public class ServicePackage : Aggregate<Guid>
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public int DurationDays { get; private set; }
        public Guid ImageId { get; private set; }
        public bool IsActive { get; private set; }

        public ServicePackage(Guid id, string name, string description, decimal price, int durationDays, Guid imageId, bool isActive)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            DurationDays = durationDays;
            ImageId = imageId;
            IsActive = isActive;
        }
    }
}

