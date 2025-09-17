using Alias.API.Aliases.Exceptions.DomainExceptions;
using Alias.API.Aliases.Models.Enums;
using BuildingBlocks.DDD;

namespace Alias.API.Aliases.Models;

public sealed class AliasAudit : Entity<Guid>, IHasCreationAudit
{
    public Guid AliasId { get; private set; }
    public AliasAuditAction Action { get; private set; }
    public string? Details { get; private set; }

    // EF Core materialization
    private AliasAudit()
    {
    }

    internal static AliasAudit Create(Guid aliasId, string action, string? details = null)
    {
        if (!Enum.TryParse<AliasAuditAction>(action, out var parsedAction))
            throw new InvalidAliasAuditActionException();

        return new AliasAudit
        {
            Id = Guid.NewGuid(),
            AliasId = aliasId,
            Action = parsedAction,
            Details = details,
        };
    }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
}