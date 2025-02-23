namespace Subscription.API.Dtos
{
    public record ServicePackageDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    Guid ImageId,
    bool IsActive
);

}
