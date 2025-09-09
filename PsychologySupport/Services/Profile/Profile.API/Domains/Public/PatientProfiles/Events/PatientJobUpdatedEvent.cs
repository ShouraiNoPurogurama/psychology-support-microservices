using BuildingBlocks.DDD;

namespace Profile.API.Domains.PatientProfiles.Events;

public record PatientJobUpdatedEvent(Guid SubjectRef, Guid? JobId) : IDomainEvent;