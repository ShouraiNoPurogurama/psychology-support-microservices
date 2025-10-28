using BuildingBlocks.DDD;

namespace UserMemory.API.Models;

public class MemoryTag : IHasCreationAudit
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;       // ex: "Topic_Food"
    public string Name { get; set; } = null!;       // ex: "Ăn uống"
    public string Category { get; set; } = null!;   // ex: "Topic"
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }

    public ICollection<UserMemory> UserMemories { get; set; } = new List<UserMemory>();
}