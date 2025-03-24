namespace Subscription.API.ServicePackages.Dtos
{
    public record CreateServicePackageDto(
        string Name,
        string Description,
        decimal Price,
        int DurationDays
    );
}
