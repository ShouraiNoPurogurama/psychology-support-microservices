using BuildingBlocks.DDD;
using Profile.API.Models.Public;

namespace Profile.API.Domains.Public.PatientProfiles.Events;

public record MedicalRecordAddedEvent(MedicalRecord MedicalRecord) : IDomainEvent;