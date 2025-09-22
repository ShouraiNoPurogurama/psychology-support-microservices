using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class IdempotencyKey
{
    public Guid Id { get; set; }

    public string IdempotencyKey1 { get; set; } = null!;

    public string RequestHash { get; set; } = null!;

    public string? ResponsePayload { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }
}
