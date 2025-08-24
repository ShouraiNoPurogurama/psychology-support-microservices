using Alias.API.Enums;

namespace Alias.API.Models;

public partial class AliasVersion
{
    public Guid Id { get; set; }

    public Guid AliasId { get; set; }

    public string AliasLabel { get; set; } = null!;

    public string AliasKey { get; set; } = null!;

    public NicknameSource NicknameSource { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual Alias Alias { get; set; } = null!;
}
