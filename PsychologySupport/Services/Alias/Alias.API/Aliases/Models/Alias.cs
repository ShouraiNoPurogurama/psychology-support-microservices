using Alias.API.Aliases.Exceptions.DomainExceptions;
using Alias.API.Aliases.Models.DomainEvents;
using Alias.API.Aliases.Models.Enums;
using Alias.API.Aliases.Models.ValueObjects;
using BuildingBlocks.DDD;

namespace Alias.API.Aliases.Models;

public sealed class Alias : AggregateRoot<Guid>, ISoftDeletable
{
    // Value Objects
    public AliasLabel Label { get; private set; } = null!;
    public AliasMetadata Metadata { get; private set; } = null!;

    // Properties
    public Guid? CurrentVersionId { get; private set; }
    public Guid? AvatarMediaId { get; private set; }
    public AliasVisibility Visibility { get; private set; } = AliasVisibility.Public;
    public AliasStatus Status { get; private set; } = AliasStatus.Active;
    public string? SuspensionReason { get; private set; }
    public DateTimeOffset? SuspendedAt { get; private set; }

    // Collections - encapsulated within the aggregate
    private readonly List<AliasVersion> _versions = new();
    private readonly List<AliasAudit> _auditRecords = new();

    public IReadOnlyList<AliasVersion> Versions => _versions.AsReadOnly();
    public IReadOnlyList<AliasAudit> AuditRecords => _auditRecords.AsReadOnly();

    // Soft Delete implementation
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public string? DeletedBy { get; set; }
    public string? DeletedByAliasId { get; set; }

    // EF Core materialization
    private Alias() { }

    public static Alias Create(
        string label,
        NicknameSource source = NicknameSource.Custom,
        AliasVisibility visibility = AliasVisibility.Public,
        bool isSystemGenerated = false)
    {
        var aliasLabel = AliasLabel.Create(label);
        var alias = new Alias
        {
            Id = Guid.NewGuid(),
            Label = aliasLabel,
            Metadata = AliasMetadata.Create(isSystemGenerated),
            Visibility = visibility,
            Status = AliasStatus.Active
        };

        // Create initial version
        var initialVersion = AliasVersion.Create(
            alias.Id,
            aliasLabel.Value,
            aliasLabel.SearchKey,
            aliasLabel.UniqueKey,
            source);

        alias._versions.Add(initialVersion);
        alias.CurrentVersionId = initialVersion.Id;

        // Create audit record
        var auditRecord = AliasAudit.Create(alias.Id, nameof(AliasAuditAction.Created), $"Initial alias created with label: {label}");
        alias._auditRecords.Add(auditRecord);

        // Emit domain events
        alias.AddDomainEvent(new AliasCreatedEvent(
            alias.Id,
            aliasLabel.Value,
            aliasLabel.UniqueKey,
            source,
            visibility,
            alias.CreatedAt));

        alias.AddDomainEvent(new AliasVersionCreatedEvent(
            alias.Id,
            initialVersion.Id,
            aliasLabel.Value,
            source,
            alias.CreatedAt));

        alias.AddDomainEvent(new AliasAuditRecordedEvent(
            alias.Id,
            AliasAuditAction.Created,
            auditRecord.Details,
            alias.CreatedAt));

        return alias;
    }

    public void UpdateLabel(string newLabel, NicknameSource source = NicknameSource.Custom)
    {
        ValidateCanBeModified();

        if (_versions.Count >= 10)
            throw new AliasVersionLimitExceededException("Cannot create more than 10 versions for an alias.");

        var oldLabel = Label.Value;
        var newAliasLabel = AliasLabel.Create(newLabel);

        // Check if label actually changed
        if (Label.Value.Equals(newLabel, StringComparison.OrdinalIgnoreCase))
            return;

        // Invalidate current version
        var currentVersion = GetCurrentVersion();
        currentVersion?.Invalidate();

        // Create new version
        var newVersion = AliasVersion.Create(
            Id,
            newAliasLabel.Value,
            newAliasLabel.SearchKey,
            newAliasLabel.UniqueKey,
            source);

        _versions.Add(newVersion);
        CurrentVersionId = newVersion.Id;
        Label = newAliasLabel;
        Metadata = Metadata.IncrementVersionCount().UpdateLastActive();

        // Create audit record
        var auditRecord = AliasAudit.Create(Id, nameof(AliasAuditAction.LabelUpdated), 
            $"Label changed from '{oldLabel}' to '{newLabel}'");
        _auditRecords.Add(auditRecord);

        // Emit domain events
        AddDomainEvent(new AliasLabelUpdatedEvent(
            Id,
            oldLabel,
            newLabel,
            newAliasLabel.UniqueKey,
            newVersion.Id,
            DateTimeOffset.UtcNow));

        AddDomainEvent(new AliasVersionCreatedEvent(
            Id,
            newVersion.Id,
            newLabel,
            source,
            DateTimeOffset.UtcNow));

        AddDomainEvent(new AliasAuditRecordedEvent(
            Id,
            AliasAuditAction.LabelUpdated,
            auditRecord.Details,
            DateTimeOffset.UtcNow));
    }

    public void ChangeVisibility(AliasVisibility newVisibility)
    {
        ValidateCanBeModified();

        if (Visibility == newVisibility) return;

        if (Status == AliasStatus.Suspended && newVisibility == AliasVisibility.Public)
            throw new AliasSuspendedException("Cannot make suspended alias public.");

        var oldVisibility = Visibility;
        Visibility = newVisibility;
        Metadata = Metadata.UpdateLastActive();

        // Create audit record
        var auditRecord = AliasAudit.Create(Id, nameof(AliasAuditAction.VisibilityChanged),
            $"Visibility changed from {oldVisibility} to {newVisibility}");
        _auditRecords.Add(auditRecord);

        AddDomainEvent(new AliasVisibilityChangedEvent(
            Id,
            oldVisibility,
            newVisibility,
            DateTimeOffset.UtcNow));

        AddDomainEvent(new AliasAuditRecordedEvent(
            Id,
            AliasAuditAction.VisibilityChanged,
            auditRecord.Details,
            DateTimeOffset.UtcNow));
    }

    public void ChangeAvatar(Guid? newAvatarMediaId)
    {
        ValidateCanBeModified();

        if (AvatarMediaId == newAvatarMediaId) return;

        var oldAvatarMediaId = AvatarMediaId;
        AvatarMediaId = newAvatarMediaId;
        Metadata = Metadata.UpdateLastActive();

        // Create audit record
        var auditRecord = AliasAudit.Create(Id, nameof(AliasAuditAction.AvatarChanged),
            $"Avatar changed from {oldAvatarMediaId} to {newAvatarMediaId}");
        _auditRecords.Add(auditRecord);

        AddDomainEvent(new AliasAvatarChangedEvent(
            Id,
            oldAvatarMediaId,
            newAvatarMediaId,
            DateTimeOffset.UtcNow));

        AddDomainEvent(new AliasAuditRecordedEvent(
            Id,
            AliasAuditAction.AvatarChanged,
            auditRecord.Details,
            DateTimeOffset.UtcNow));
    }

    public void Suspend(string reason, Guid suspendedBy)
    {
        if (Status == AliasStatus.Suspended) return;

        ValidateCanBeModified();

        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidAliasDataException("Suspension reason is required.");

        Status = AliasStatus.Suspended;
        Visibility = AliasVisibility.Suspended;
        SuspensionReason = reason;
        SuspendedAt = DateTimeOffset.UtcNow;

        // Invalidate all versions when suspended
        foreach (var version in _versions.Where(v => v.IsActive))
        {
            version.Invalidate();
        }

        // Create audit record
        var auditRecord = AliasAudit.Create(Id, nameof(AliasAuditAction.Suspended), 
            $"Alias suspended. Reason: {reason}");
        _auditRecords.Add(auditRecord);

        AddDomainEvent(new AliasSuspendedEvent(
            Id,
            reason,
            suspendedBy,
            DateTimeOffset.UtcNow));

        AddDomainEvent(new AliasAuditRecordedEvent(
            Id,
            AliasAuditAction.Suspended,
            auditRecord.Details,
            DateTimeOffset.UtcNow));
    }

    public void Restore(Guid restoredBy)
    {
        if (Status != AliasStatus.Suspended)
            throw new InvalidAliasDataException("Only suspended aliases can be restored.");

        Status = AliasStatus.Active;
        Visibility = AliasVisibility.Public;
        SuspensionReason = null;
        SuspendedAt = null;

        // Reactivate current version if exists
        var currentVersion = GetCurrentVersion();
        if (currentVersion != null && !currentVersion.IsActive)
        {
            // Create a new version based on the current label to reactivate
            var newVersion = AliasVersion.Create(
                Id,
                Label.Value,
                Label.SearchKey,
                Label.UniqueKey,
                NicknameSource.Custom);

            _versions.Add(newVersion);
            CurrentVersionId = newVersion.Id;
        }

        Metadata = Metadata.UpdateLastActive();

        // Create audit record
        var auditRecord = AliasAudit.Create(Id, nameof(AliasAuditAction.Restored), "Alias restored from suspension");
        _auditRecords.Add(auditRecord);

        AddDomainEvent(new AliasRestoredEvent(
            Id,
            restoredBy,
            DateTimeOffset.UtcNow));

        AddDomainEvent(new AliasAuditRecordedEvent(
            Id,
            AliasAuditAction.Restored,
            auditRecord.Details,
            DateTimeOffset.UtcNow));
    }

    public void RecordActivity()
    {
        if (Status == AliasStatus.Active && !IsDeleted)
        {
            Metadata = Metadata.UpdateLastActive();
        }
    }

    public void Delete(Guid deleterAliasId)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByAliasId = deleterAliasId.ToString();
        Status = AliasStatus.Suspended;
        Visibility = AliasVisibility.Private;

        // Invalidate all versions
        foreach (var version in _versions.Where(v => v.IsActive))
        {
            version.Invalidate();
        }

        // Create audit record
        var auditRecord = AliasAudit.Create(Id, nameof(AliasAuditAction.Deleted), "Alias soft deleted");
        _auditRecords.Add(auditRecord);

        AddDomainEvent(new AliasAuditRecordedEvent(
            Id,
            AliasAuditAction.Deleted,
            auditRecord.Details,
            DateTimeOffset.UtcNow));
    }

    private AliasVersion? GetCurrentVersion()
    {
        return CurrentVersionId.HasValue
            ? _versions.FirstOrDefault(v => v.Id == CurrentVersionId.Value)
            : _versions.OrderByDescending(v => v.ValidFrom).FirstOrDefault();
    }

    private void ValidateCanBeModified()
    {
        if (IsDeleted)
            throw new InvalidAliasDataException("Cannot modify deleted alias.");

        if (Status == AliasStatus.Banned)
            throw new AliasSuspendedException("Cannot modify banned alias.");
    }

    //Computed properties
    public bool IsActive => Status == AliasStatus.Active && !IsDeleted;
    public bool IsSuspended => Status == AliasStatus.Suspended;
    public bool HasAvatar => AvatarMediaId.HasValue;
    public bool IsPublic => Visibility == AliasVisibility.Public;
    public AliasVersion? CurrentVersion => GetCurrentVersion();
    public int TotalVersions => _versions.Count;
    public bool IsSystemGenerated => Metadata.IsSystemGenerated;
}
