using Profile.API.Domains.MentalDisorders.Dtos;
using Profile.API.Domains.PatientProfiles.Enum;

namespace Profile.API.Domains.PatientProfiles.Dtos;

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