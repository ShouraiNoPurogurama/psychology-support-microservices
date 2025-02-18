using Profile.API.Common.ValueObjects;
using Profile.API.PatientProfiles.Models;
using Profile.API.PatientProfiles.ValueObjects;

namespace Profile.API.PatientProfiles.Dtos;

public record GetPatientProfileDto(
    Guid UserId,
    string? Gender,
    string? Allergies,
    PersonalityTrait PersonalityTraits,
    ContactInfo ContactInfo,
    MedicalHistoryDto? MedicalHistory,
    IEnumerable<MedicalRecord> MedicalRecords
);