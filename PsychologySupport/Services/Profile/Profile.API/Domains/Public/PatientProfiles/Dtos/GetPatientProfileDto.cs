using BuildingBlocks.Data.Common;
using Profile.API.Models.Public;

namespace Profile.API.Domains.Public.PatientProfiles.Dtos;

public record GetPatientProfileDto(
    Guid Id,
    Guid UserId,
    string FullName,
    UserGender Gender,
    string? Allergies,
    PersonalityTrait PersonalityTraits,
    MedicalHistoryDto? MedicalHistory,
    Job? Job,
    DateOnly BirthDate,
    IEnumerable<MedicalRecordDto> MedicalRecords
);