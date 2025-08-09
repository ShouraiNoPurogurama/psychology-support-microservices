using BuildingBlocks.DDD;

namespace Subscription.API.ServicePackages.Models;

public class ServicePackage : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public int DurationDays { get; private set; }
    public Guid ImageId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ServicePackage()
    {
    }

    private ServicePackage(Guid id, string name, string description, decimal price, int durationDays, Guid imageId, bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        DurationDays = durationDays;
        ImageId = imageId;
        IsActive = isActive;
    }

    public static ServicePackage Create(Guid id,
        string name,
        string description,
        decimal price,
        int durationDays,
        Guid imageId,
        bool isActive)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentException.ThrowIfNullOrEmpty(description, nameof(description));
        ArgumentException.ThrowIfNullOrEmpty(id.ToString(), nameof(id));
        ArgumentException.ThrowIfNullOrEmpty(imageId.ToString(), nameof(imageId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price, nameof(price));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationDays, nameof(durationDays));
        
        return new ServicePackage(id, name, description, price, durationDays, imageId, isActive);
    }
    
    public void Update(string? name, string? description, decimal? price, int? durationDays, bool? isActive)
    {
        if (name is not null)
            Name = name;

        if (description is not null)
            Description = description;

        if (price.HasValue)
            Price = price.Value;

        if (durationDays.HasValue)
            DurationDays = durationDays.Value;

        if (isActive.HasValue)
            IsActive = isActive.Value;
    }
}