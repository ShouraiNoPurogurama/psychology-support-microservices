using System;
using System.Collections.Generic;
using BuildingBlocks.DDD;

namespace Alias.API.Models.Pii;

public partial class AliasOwnerMap : Entity<Guid>, IHasCreationAudit
{
    public Guid AliasId { get; set; }

    public Guid UserId { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
