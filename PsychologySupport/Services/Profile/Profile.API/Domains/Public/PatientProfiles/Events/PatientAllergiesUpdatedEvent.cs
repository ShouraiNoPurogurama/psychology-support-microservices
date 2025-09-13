using BuildingBlocks.DDD;

namespace Profile.API.Domains.Public.PatientProfiles.Events;

public record PatientAllergiesUpdatedEvent(Guid PatientProfileId, string? Allergies) : IDomainEvent;