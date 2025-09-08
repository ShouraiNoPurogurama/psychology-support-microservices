using BuildingBlocks.DDD;

namespace DigitalGoods.API.Models;

public class EmotionTag : AuditableEntity<Guid>
{
    public string Code { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public Guid? MediaId { get; set; }
    
    public string? Topic { get; set; }
    
    public int SortOrder { get; set; }
    
    public bool IsActive { get; set; }

    public ICollection<DigitalGood> DigitalGoods { get; set; } = new List<DigitalGood>();
}