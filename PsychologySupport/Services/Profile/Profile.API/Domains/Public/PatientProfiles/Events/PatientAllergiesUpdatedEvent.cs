using BuildingBlocks.DDD;

namespace Profile.API.Domains.PatientProfiles.Events;

public record PatientAllergiesUpdatedEvent(Guid SubjectRef, string? Allergies) : IDomainEvent;