using Profile.API.MentalDisorders.Dtos;

namespace Profile.API.PatientProfiles.Dtos;

public record MedicalHistoryDto(
    string Description,
    DateTimeOffset DiagnosedAt,
    IReadOnlyList<SpecificMentalDisorderDto> SpecificMentalDisorders,
    IReadOnlyList<PhysicalSymptomDto> PhysicalSymptoms
);