namespace Alias.API.Models;

public partial class AliasOwnerMap
{
    public Guid Id { get; set; }

    public Guid AliasId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
