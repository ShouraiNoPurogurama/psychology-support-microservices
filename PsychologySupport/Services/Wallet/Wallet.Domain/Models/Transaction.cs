using System;
using System.Collections.Generic;

namespace Wallet.Domain.Models;

public partial class Transaction
{
    public Guid Id { get; set; }

    public Guid UserAliasId { get; set; }

    public string Type { get; set; } = null!;

    public int PointAmount { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? DigitalGoodId { get; set; }

    public int BalanceAfter { get; set; }

    public string? Description { get; set; }

    public Guid? IdempotencyKeyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual ICollection<Balance> Balances { get; set; } = new List<Balance>();

    public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
}
