namespace Subscription.API.ServicePackages.Dtos;

public record ServicePackageDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    Guid ImageId,
    bool IsActive,
    bool? IsPurchased = null
)
{
    public bool? IsPurchased { get; set; } = IsPurchased;
}