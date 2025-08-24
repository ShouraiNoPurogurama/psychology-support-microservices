namespace Alias.API.Models;

public partial class AliasAudit
{
    public Guid Id { get; set; }

    public Guid AliasId { get; set; }

    public string Action { get; set; } = null!;

    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
