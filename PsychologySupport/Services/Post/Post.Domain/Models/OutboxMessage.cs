namespace Post.Domain.Models;

public partial class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime OccuredOn { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
