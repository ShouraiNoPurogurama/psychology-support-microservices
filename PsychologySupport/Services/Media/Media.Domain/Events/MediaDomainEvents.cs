namespace Media.Domain.Events;

public sealed record MediaUploadedEvent(
    Guid MediaId,
    string MediaPurpose,
    string MimeType,
    long SizeInBytes,
    string ChecksumSha256,
    int? Width,
    int? Height,
    DateTimeOffset UploadedAt
) : IDomainEvent;

public sealed record MediaProcessingStartedEvent(
    Guid MediaId,
    string JobType
) : IDomainEvent;

public sealed record MediaProcessingCompletedEvent(
    Guid MediaId,
    string JobType,
    bool Success,
    string? ErrorMessage = null
) : IDomainEvent;

public sealed record MediaModerationCompletedEvent(
    Guid MediaId,
    string Status,
    decimal? Score,
    string PolicyVersion,
    DateTimeOffset CompletedAt
) : IDomainEvent;

public record MediaModerationSucceededEvent(Guid Id, 
    string Status, 
    decimal? Score, 
    string PolicyVersion, 
    DateTime UtcNow)
    : IDomainEvent;

public sealed record MediaStateChangedEvent(
    Guid MediaId,
    string PreviousState,
    string NewState,
    string? Reason = null
) : IDomainEvent;

public sealed record MediaVariantCreatedEvent(
    Guid MediaId,
    Guid VariantId,
    string VariantType,
    string Format,
    int Width,
    int Height,
    string CdnUrl
) : IDomainEvent;

public sealed record MediaOwnershipAssignedEvent(
    Guid MediaId,
    string OwnerType,
    Guid OwnerId
) : IDomainEvent;

public sealed record MediaDeletedEvent(
    Guid MediaId,
    string Reason,
    DateTimeOffset DeletedAt
) : IDomainEvent;