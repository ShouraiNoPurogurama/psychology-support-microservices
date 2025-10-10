using System;
using System.Collections.Generic;

namespace Wallet.Domain.Models;

public partial class TransactionDetail
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }

    public Guid DigitalGoodId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPointAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
