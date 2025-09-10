using Profile.API.Domains.Public.MentalDisorders.Dtos;
using Profile.API.Domains.Public.PatientProfiles.Enum;

namespace Profile.API.Domains.Public.PatientProfiles.Dtos;

public record MedicalRecordDto(
    Guid Id,
    Guid PatientProfileId,
    Guid DoctorProfileId,
    MedicalHistoryDto? MedicalHistory,
    string Notes,
    MedicalRecordStatus Status,
    IReadOnlyList<SpecificMentalDisorderDto> SpecificMentalDisorders,
    DateTimeOffset? CreatedAt
);