using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.DDD;

public abstract class Entity<T> : IEntity<T>
{
    [Key]
    public T Id { get; set; }
    
    public DateTimeOffset? CreatedAt { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public DateTimeOffset? LastModified { get; set; }
    
    public string? LastModifiedBy { get; set; }
}