using BuildingBlocks.DDD;

namespace DigitalGoods.API.Models;

public partial class EmotionTag : AuditableEntity<Guid>
{
    public Guid? DigitalGoodId { get; set; }
    
    public Guid? MediaId { get; set; }
    
    public string Code { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? Icon { get; set; }

    public string? Color { get; set; }
    
    public bool IsActive { get; set; }
    
    public int SortOrder { get; set; }

    public string? UnicodeCodepoint { get; set; }

    public string? Topic { get; set; }
    
    public ICollection<DigitalGood> DigitalGoods { get; set; } = new List<DigitalGood>();
}