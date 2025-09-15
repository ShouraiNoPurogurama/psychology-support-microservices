namespace Post.Domain.Events;

public record PostApprovedEvent(Guid PostId, Guid ModeratorId) : IDomainEvent;