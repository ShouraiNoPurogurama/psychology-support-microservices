namespace Promotion.Grpc.Models;

public class PromotionType
{
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    public virtual ICollection<Promotion> Promotions { get; set; } = default!;
    
    public virtual ICollection<PromotionTypeServicePackage> PromotionTypeServicePackages { get; set; } = default!;
}