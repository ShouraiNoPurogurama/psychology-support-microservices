using BuildingBlocks.DDD;
using Pgvector;

namespace UserMemory.API.Models;

public class UserMemory : AuditableEntity<Guid>
{
    public Guid AliasId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
    public ICollection<MemoryTag> MemoryTags { get; set; } = new List<MemoryTag>();
}