using BuildingBlocks.DDD;

namespace Profile.API.Domains.PatientProfiles.Events;

public record PatientProfileUpdatedEvent(
    Guid SubjectRef, 
    string? Allergies, 
    PersonalityTrait PersonalityTraits, 
    Guid? JobId) : IDomainEvent;