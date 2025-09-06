using System;
using System.Collections.Generic;
using Alias.API.Domains.Aliases.Enums;
using BuildingBlocks.DDD;

namespace Alias.API.Models.Public;

public partial class AliasVersion : Entity<Guid>, IHasCreationAudit
{
    public Guid AliasId { get; set; }

    public string Label { get; set; } = null!;

    public string SearchKey { get; set; } = null!;
    
    public string UniqueKey { get; set; } = null!;

    public NicknameSource NicknameSource { get; set; }
    
    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public virtual Alias Alias { get; set; } = null!;
    
    public DateTimeOffset? CreatedAt { get; set; }
    
    public string? CreatedBy { get; set; }
}
