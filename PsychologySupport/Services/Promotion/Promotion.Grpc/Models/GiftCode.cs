namespace Promotion.Grpc.Models;

public class GiftCode
{
    public string Id { get; set; }

    public string Code { get; set; }

    public string PatientId { get; set; }
    
    public string PromotionId { get; set; }
    
    public decimal MoneyValue { get; set; } // VND
    
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public virtual Promotion Promotion { get; set; } = default!;
}