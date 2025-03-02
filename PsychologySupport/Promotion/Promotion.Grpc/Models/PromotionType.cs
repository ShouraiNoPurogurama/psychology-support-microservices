namespace Promotion.Grpc.Models;

public class PromotionType
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; } = string.Empty;
    
    public virtual ICollection<Promotion> Promotions { get; set; } = default!;
}