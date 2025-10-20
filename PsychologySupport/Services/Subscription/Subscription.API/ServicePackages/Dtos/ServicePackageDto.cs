namespace Subscription.API.ServicePackages.Dtos;

public record ServicePackageDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,           // Giá ưu đãi
    decimal OriginalPrice,   // Giá gốc
    string? DiscountLabel,   // Nhãn giảm
    int DurationDays,
    Guid ImageId,
    bool IsActive
)
{
    public string? PurchaseStatus { get; set; }
    public decimal? UpgradePrice { get; set; }

    public ServicePackageDto() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        0,
        0,
        null,
        0,
        Guid.Empty,
        false
    )
    {
    }
}
