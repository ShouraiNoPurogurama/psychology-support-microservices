using BuildingBlocks.DDD;

namespace Alias.API.Models.Public;

public partial class Alias : AuditableEntity<Guid>
{
    public Guid? CurrentVersionId { get; set; }
    
    public Guid? AvatarMediaId { get; set; }

    public virtual ICollection<AliasVersion> AliasVersions { get; set; } = new List<AliasVersion>();
}
