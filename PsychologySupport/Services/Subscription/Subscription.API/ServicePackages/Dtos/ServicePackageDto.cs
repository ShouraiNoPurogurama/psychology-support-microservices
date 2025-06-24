public record ServicePackageDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int DurationDays,
    Guid ImageId,
    bool IsActive
)
{
    public string? PurchaseStatus { get; set; }
    public decimal? UpgradePrice { get; set; }

    public ServicePackageDto() : this(Guid.Empty, string.Empty, string.Empty, 0, 0, Guid.Empty, false)
    {
    }
}