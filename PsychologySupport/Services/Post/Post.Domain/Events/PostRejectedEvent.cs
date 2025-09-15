namespace Post.Domain.Events;

public record ostRejectedEvent(Guid Id, List<string> RejectionReasons) : IDomainEvent;