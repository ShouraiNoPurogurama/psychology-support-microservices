using Profile.API.MentalDisorders.Dtos;
using Profile.API.PatientProfiles.Enum;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Dtos;

public record MedicalRecordDto(
    Guid Id,
    Guid PatientProfileId,
    Guid DoctorProfileId,
    MedicalHistoryDto? MedicalHistory,
    string Notes,
    MedicalRecordStatus Status,
    IReadOnlyList<SpecificMentalDisorderDto> SpecificMentalDisorders
);