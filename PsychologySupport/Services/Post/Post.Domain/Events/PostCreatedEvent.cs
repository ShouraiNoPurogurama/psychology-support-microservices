namespace Post.Domain.Events;

public record PostCreatedEvent(Guid PostId, Guid AuthorAliasId) : IDomainEvent;