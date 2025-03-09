namespace Promotion.Grpc.Models;

public class PromotionTypeServicePackage
{
    public string PromotionTypeId { get; set; }
    
    public string ServicePackageId { get; set; }

    public virtual PromotionType PromotionType { get; set; } = default!;
}