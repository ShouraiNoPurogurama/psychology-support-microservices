using BuildingBlocks.DDD;

namespace Profile.API.Domains.Public.PatientProfiles.Events;

public record PatientAllergiesUpdatedEvent(Guid SubjectRef, string? Allergies) : IDomainEvent;