using BuildingBlocks.DDD;

namespace Profile.API.Domains.Public.PatientProfiles.Events;

public record PatientPersonalityUpdatedEvent(Guid PatientProfileId, PersonalityTrait PersonalityTraits) : IDomainEvent;