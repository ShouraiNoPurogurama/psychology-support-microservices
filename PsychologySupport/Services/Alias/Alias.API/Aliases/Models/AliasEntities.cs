using Alias.API.Aliases.Models.Enums;
using BuildingBlocks.DDD;

namespace Alias.API.Aliases.Models;

public sealed class AliasVersion : Entity<Guid>, IHasCreationAudit
{
    public Guid AliasId { get; private set; }
    public string DisplayName { get; private set; } = null!;
    public string SearchKey { get; private set; } = null!;
    public string UniqueKey { get; private set; } = null!;
    public NicknameSource NicknameSource { get; private set; }
    public DateTimeOffset ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }

    // EF Core materialization
    private AliasVersion()
    {
    }

    internal static AliasVersion Create(
        Guid aliasId,
        string label,
        string searchKey,
        string uniqueKey,
        NicknameSource source)
    {
        return new AliasVersion
        {
            Id = Guid.NewGuid(),
            AliasId = aliasId,
            DisplayName = label,
            SearchKey = searchKey,
            UniqueKey = uniqueKey,
            NicknameSource = source,
            ValidFrom = DateTimeOffset.UtcNow,
            ValidTo = null
        };
    }

    internal void Invalidate()
    {
        ValidTo = DateTimeOffset.UtcNow;
    }

    public bool IsActive => ValidTo == null;
    public bool IsValid => ValidTo == null || ValidTo > DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}