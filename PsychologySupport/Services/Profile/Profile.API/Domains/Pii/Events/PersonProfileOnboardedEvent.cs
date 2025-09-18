using BuildingBlocks.DDD;

namespace Profile.API.Domains.Pii.Events;

public record PersonProfileOnboardedEvent(Guid SubjectRef) : IDomainEvent;