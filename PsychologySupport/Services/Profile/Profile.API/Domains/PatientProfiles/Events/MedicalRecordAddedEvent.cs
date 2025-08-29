using BuildingBlocks.DDD;
using Profile.API.Domains.PatientProfiles.Models;

namespace Profile.API.Domains.PatientProfiles.Events;

public record MedicalRecordAddedEvent(MedicalRecord MedicalRecord) : IDomainEvent;