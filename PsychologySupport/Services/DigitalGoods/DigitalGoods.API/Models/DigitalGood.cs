using BuildingBlocks.DDD;
using System;
using System.Collections.Generic;

namespace DigitalGoods.API.Models;

public partial class DigitalGood : Entity<Guid>
{
    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string ConsumptionType { get; set; } = null!;

    public int Price { get; set; }

    public string? Description { get; set; }

    public Guid? MediaId { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    
    public ICollection<EmotionTag> EmotionTags { get; set; } = new List<EmotionTag>();
}
