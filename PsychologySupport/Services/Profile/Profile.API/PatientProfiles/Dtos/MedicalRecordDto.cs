using Profile.API.MentalDisorders.Dtos;
using Profile.API.PatientProfiles.Enum;

namespace Profile.API.PatientProfiles.Dtos;

public record MedicalRecordDto(
    Guid Id,
    Guid PatientProfileId,
    Guid DoctorProfileId,
    Guid? MedicalHistoryId,
    string Notes,
    MedicalRecordStatus Status,
    IReadOnlyList<SpecificMentalDisorderDto> SpecificMentalDisorders
);