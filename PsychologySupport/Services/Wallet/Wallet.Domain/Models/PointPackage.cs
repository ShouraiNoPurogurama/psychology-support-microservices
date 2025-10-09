using System;
using System.Collections.Generic;

namespace Wallet.Domain.Models;

public partial class PointPackage
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int PointAmount { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
