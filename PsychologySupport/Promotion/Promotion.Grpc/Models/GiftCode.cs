namespace Promotion.Grpc.Models;

public class GiftCode
{
    public Guid Id { get; set; }
    
    public Guid PromotionId { get; set; }
    
    public decimal MoneyValue { get; set; } // VND
    
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; }
    
    public virtual Promotion Promotion { get; set; } = default!;
}