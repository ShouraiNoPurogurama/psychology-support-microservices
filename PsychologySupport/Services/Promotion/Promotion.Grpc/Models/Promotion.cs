
namespace Promotion.Grpc.Models;

public class Promotion
{
    public string Id { get; set; }

    public string Name { get; set; } 

    public string PromotionTypeId { get; set; }
    
    public string ImageId { get; set; }
    
    public DateTimeOffset EffectiveDate { get; set; }
    
    public DateTimeOffset EndDate { get; set; }
    
    public bool IsActive { get; set; }
    
    public virtual PromotionType PromotionType { get; set; } = default!;
    
    public virtual ICollection<PromoCode> PromoCodes { get; set; } = [];
    
    public virtual ICollection<GiftCode> GiftCodes { get; set; } = [];

    public virtual ICollection<PromotionServicePackage> PromotionServicePackages { get; set; } = default!;
}