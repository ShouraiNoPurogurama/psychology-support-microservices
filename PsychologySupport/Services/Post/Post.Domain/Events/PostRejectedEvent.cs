namespace Post.Domain.Events;

public record PostRejectedEvent(Guid Id, List<string> RejectionReasons) : IDomainEvent;