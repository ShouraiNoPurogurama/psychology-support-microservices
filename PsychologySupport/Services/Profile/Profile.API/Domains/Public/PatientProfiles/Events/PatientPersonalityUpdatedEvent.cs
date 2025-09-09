using BuildingBlocks.DDD;

namespace Profile.API.Domains.PatientProfiles.Events;

public record PatientPersonalityUpdatedEvent(Guid SubjectRef, PersonalityTrait PersonalityTraits) : IDomainEvent;