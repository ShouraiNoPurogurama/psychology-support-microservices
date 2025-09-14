using BuildingBlocks.DDD;

namespace Profile.API.Domains.Public.PatientProfiles.Events;

public record PatientProfileOnboardedEvent(Guid PatientId) : IDomainEvent;