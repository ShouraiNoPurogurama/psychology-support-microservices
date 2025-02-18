using BuildingBlocks.DDD;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Events;

public record MedicalRecordAddedEvent(MedicalRecord MedicalRecord) : IDomainEvent;