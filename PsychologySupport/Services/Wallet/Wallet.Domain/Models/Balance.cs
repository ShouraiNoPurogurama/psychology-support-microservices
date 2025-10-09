using System;
using System.Collections.Generic;

namespace Wallet.Domain.Models;

public partial class Balance
{
    public Guid UserAliasId { get; set; }

    public int Balance1 { get; set; }

    public Guid LastTransactionId { get; set; }

    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual Transaction LastTransaction { get; set; } = null!;
}
