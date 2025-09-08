namespace Post.Domain.Events;

public record PostDeletedEvent(Guid Id) : IDomainEvent;