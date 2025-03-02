
namespace Promotion.Grpc.Models;

public class Promotion
{
    public Guid Id { get; set; }
    
    public Guid PromotionTypeId { get; set; }
    
    public Guid ImageId { get; set; }
    
    public DateTimeOffset EffectiveDate { get; set; }
    
    public DateTimeOffset EndDate { get; set; }
    
    public bool IsActive { get; set; }
    
    public virtual PromotionType PromotionType { get; set; } = default!;
    
    public virtual ICollection<PromoCode> PromoCodes { get; set; } = default!;
    
    public virtual ICollection<GiftCode> GiftCodes { get; set; } = default!;
}