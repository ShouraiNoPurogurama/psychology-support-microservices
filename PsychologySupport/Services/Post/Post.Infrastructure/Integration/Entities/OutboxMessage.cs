using Post.Domain.Abstractions;

namespace Post.Infrastructure.Integration.Entities;

public partial class OutboxMessage: Entity<Guid>
{
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
}
