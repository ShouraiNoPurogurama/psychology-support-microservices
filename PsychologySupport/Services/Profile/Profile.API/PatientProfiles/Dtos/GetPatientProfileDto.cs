using BuildingBlocks.Enums;
using Profile.API.PatientProfiles.Enum;

namespace Profile.API.PatientProfiles.Dtos;

public record GetPatientProfileDto(
    Guid Id,
    Guid UserId,
    UserGender Gender,
    string? Allergies,
    PersonalityTrait PersonalityTraits,
    ContactInfo ContactInfo,
    MedicalHistoryDto? MedicalHistory,
    IEnumerable<MedicalRecordDto> MedicalRecords
);