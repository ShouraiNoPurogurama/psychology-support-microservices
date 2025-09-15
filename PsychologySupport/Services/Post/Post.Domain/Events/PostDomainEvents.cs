using Post.Domain.Enums;

namespace Post.Domain.Events;

public record PostContentUpdatedEvent(Guid PostId, string OldContent, string NewContent) : IDomainEvent;
public record PostVisibilityChangedEvent(Guid PostId, PostVisibility OldVisibility, PostVisibility NewVisibility) : IDomainEvent;
public record PostRejectedEvent(Guid PostId, IReadOnlyList<string> Reasons, Guid ModeratorId) : IDomainEvent;
public record PostMediaAddedEvent(Guid PostId, Guid MediaId) : IDomainEvent;
public record PostMediaRemovedEvent(Guid PostId, Guid MediaId) : IDomainEvent;
public record PostCategoryAddedEvent(Guid PostId, Guid CategoryTagId) : IDomainEvent;
public record PostCategoryRemovedEvent(Guid PostId, Guid CategoryTagId) : IDomainEvent;
public record PostMetricsUpdatedEvent(Guid PostId, string MetricType, int Delta) : IDomainEvent;
public record PostViewedEvent(Guid PostId) : IDomainEvent;
public record PostRestoredEvent(Guid PostId, Guid RestorerAliasId) : IDomainEvent;

// Gift Domain Events
public record GiftSentEvent(Guid GiftId, string TargetType, Guid TargetId, Guid GiftItemId, Guid SenderAliasId) : IDomainEvent;
public record GiftMessageUpdatedEvent(Guid GiftId, string? NewMessage) : IDomainEvent;

// CategoryTag Domain Events
public record CategoryTagCreatedEvent(Guid CategoryTagId, string Code, string DisplayName) : IDomainEvent;
public record CategoryTagUpdatedEvent(Guid CategoryTagId, string Code, string OldDisplayName, string NewDisplayName) : IDomainEvent;
public record CategoryTagStatusChangedEvent(Guid CategoryTagId, string Code, bool IsActive) : IDomainEvent;
