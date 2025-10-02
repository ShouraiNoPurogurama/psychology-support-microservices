using BuildingBlocks.DDD;
using System;
using System.Collections.Generic;

namespace DigitalGoods.API.Models;

public partial class Inventory : Entity<Guid>
{
    public Guid Subject_ref { get; set; }

    public Guid DigitalGoodId { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = null!;

    public DateTimeOffset GrantedAt { get; set; }

    public DateTimeOffset? ExpiredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual DigitalGood DigitalGood { get; set; } = null!;
}
