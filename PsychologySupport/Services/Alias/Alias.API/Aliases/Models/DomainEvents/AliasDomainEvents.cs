using Alias.API.Aliases.Models.Enums;
using BuildingBlocks.DDD;

namespace Alias.API.Aliases.Models.DomainEvents;

public sealed record AliasCreatedEvent(
    Guid AliasId,
    string Label,
    string UniqueKey,
    NicknameSource Source,
    AliasVisibility Visibility,
    DateTimeOffset CreatedAt
) : IDomainEvent;

public sealed record AliasLabelUpdatedEvent(
    Guid AliasId,
    string OldLabel,
    string NewLabel,
    string NewUniqueKey,
    Guid NewVersionId,
    DateTimeOffset UpdatedAt
) : IDomainEvent;

public sealed record AliasVisibilityChangedEvent(
    Guid AliasId,
    AliasVisibility OldVisibility,
    AliasVisibility NewVisibility,
    DateTimeOffset ChangedAt
) : IDomainEvent;

public sealed record AliasAvatarChangedEvent(
    Guid AliasId,
    Guid? OldAvatarMediaId,
    Guid? NewAvatarMediaId,
    DateTimeOffset ChangedAt
) : IDomainEvent;

public sealed record AliasSuspendedEvent(
    Guid AliasId,
    string Reason,
    Guid SuspendedBy,
    DateTimeOffset SuspendedAt
) : IDomainEvent;

public sealed record AliasRestoredEvent(
    Guid AliasId,
    Guid RestoredBy,
    DateTimeOffset RestoredAt
) : IDomainEvent;

public sealed record AliasVersionCreatedEvent(
    Guid AliasId,
    Guid VersionId,
    string Label,
    NicknameSource Source,
    DateTimeOffset CreatedAt
) : IDomainEvent;

public sealed record AliasAuditRecordedEvent(
    Guid AliasId,
    AliasAuditAction Action,
    string? Details,
    DateTimeOffset RecordedAt
) : IDomainEvent;
