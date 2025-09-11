using BuildingBlocks.DDD;

namespace Profile.API.Domains.Public.PatientProfiles.Events;

public record PatientProfileCreatedEvent(Guid PatientProfileId) : IDomainEvent;
