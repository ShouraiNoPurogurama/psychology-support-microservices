namespace Media.Domain.Events
{
    public record MediaModerationRequestedEvent(Guid MediaId) : IDomainEvent;
}
