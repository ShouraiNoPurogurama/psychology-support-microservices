namespace DigitalGoods.API.Models;

public partial class Inventory
{
    public Guid InventoryId { get; set; }

    public Guid Subject_ref { get; set; }

    public Guid DigitalGoodId { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = null!;

    public DateTime GrantedAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual DigitalGood DigitalGood { get; set; } = null!;
}
