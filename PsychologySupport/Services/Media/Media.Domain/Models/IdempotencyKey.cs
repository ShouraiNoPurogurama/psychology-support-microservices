using System;
using System.Collections.Generic;

namespace Media.API.Media.Models;

public partial class IdempotencyKey
{
    public Guid Id { get; set; }

    public string IdempotencyKey1 { get; set; } = null!;

    public string RequestHash { get; set; } = null!;

    public string? ResponseBody { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
