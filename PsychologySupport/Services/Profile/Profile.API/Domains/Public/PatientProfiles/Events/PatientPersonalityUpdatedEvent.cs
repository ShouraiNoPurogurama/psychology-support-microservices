using BuildingBlocks.DDD;

namespace Profile.API.Domains.Public.PatientProfiles.Events;

public record PatientPersonalityUpdatedEvent(Guid SubjectRef, PersonalityTrait PersonalityTraits) : IDomainEvent;