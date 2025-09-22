using System.Text.Json;
using Alias.API.Aliases.Exceptions.DomainExceptions;
using Alias.API.Aliases.Models.Aliases.Enums;
using BuildingBlocks.DDD;

namespace Alias.API.Aliases.Models.Aliases;

public sealed class AliasAudit : Entity<Guid>, IHasCreationAudit
{
    public Guid AliasId { get; private set; }
    public AliasAuditAction Action { get; private set; }
    public string? Details { get; private set; }

    // EF Core materialization
    private AliasAudit()
    {
    }

    internal static AliasAudit Create<TDetails>(Guid aliasId, string action, TDetails? details = null)
        where TDetails : class
    {
        if (!Enum.TryParse<AliasAuditAction>(action, out var parsedAction))
            throw new InvalidAliasAuditActionException();

        return new AliasAudit
        {
            Id = Guid.NewGuid(),
            AliasId = aliasId,
            Action = parsedAction,
            Details = details is not null
                ? JsonSerializer.Serialize(details, new JsonSerializerOptions { WriteIndented = false })
                : null,
        };
    }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
}