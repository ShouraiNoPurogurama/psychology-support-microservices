using BuildingBlocks.DDD;

namespace Profile.API.Domains.Public.PatientProfiles.Events;

public record PatientJobUpdatedEvent(Guid PatientProfileId, Guid? JobId) : IDomainEvent;