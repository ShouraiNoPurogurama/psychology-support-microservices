using Alias.API.Aliases.Models.Aliases.Enums;
using BuildingBlocks.DDD;

namespace Alias.API.Aliases.Models.Aliases.DomainEvents;

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
    object Details,
    DateTimeOffset RecordedAt
) : IDomainEvent;

/**
 * Follow Domain Events
 * ====================
 * 
 * Domain events related to follow relationships between aliases.
 * These events are published when follow relationships are created or removed
 * and can trigger side effects like updating follower counts or sending notifications.
 */

/// <summary>
/// Published when a new follow relationship is created between two aliases.
/// </summary>
/// <param name="FollowId">The unique identifier of the follow relationship</param>
/// <param name="FollowerAliasId">The ID of the alias that is following</param>
/// <param name="FollowedAliasId">The ID of the alias being followed</param>
/// <param name="FollowedAt">The timestamp when the follow relationship was created</param>
public sealed record FollowCreatedDomainEvent(
    Guid FollowId,
    Guid FollowerAliasId,
    Guid FollowedAliasId,
    DateTimeOffset FollowedAt
) : IDomainEvent;

/// <summary>
/// Published when a follow relationship is removed between two aliases.
/// </summary>
/// <param name="FollowId">The unique identifier of the follow relationship that was removed</param>
/// <param name="FollowerAliasId">The ID of the alias that was following</param>
/// <param name="FollowedAliasId">The ID of the alias that was being followed</param>
/// <param name="UnfollowedAt">The timestamp when the follow relationship was removed</param>
public sealed record FollowRemovedDomainEvent(
    Guid FollowId,
    Guid FollowerAliasId,
    Guid FollowedAliasId,
    DateTimeOffset UnfollowedAt
) : IDomainEvent;
