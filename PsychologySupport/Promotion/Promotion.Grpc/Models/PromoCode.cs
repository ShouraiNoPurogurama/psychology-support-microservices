namespace Promotion.Grpc.Models;

public class PromoCode
{
    public string Id { get; set; }
    
    public string PromotionId { get; set; }
    public string Code { get; set; } = string.Empty;
    
    public int Value { get; set; } // Percentage discount
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; }
    
    public virtual Promotion Promotion { get; set; } = default!;
}