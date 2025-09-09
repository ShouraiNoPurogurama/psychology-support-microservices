using BuildingBlocks.DDD;

namespace Profile.API.Domains.PatientProfiles.Events;

public record PatientProfileCreatedEvent(Guid SubjectRef) : IDomainEvent;
