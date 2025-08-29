using Profile.API.Domains.MentalDisorders.Dtos;

namespace Profile.API.Domains.PatientProfiles.Dtos;

public record MedicalHistoryDto(
    string Description,
    DateTimeOffset DiagnosedAt,
    IReadOnlyList<SpecificMentalDisorderDto> SpecificMentalDisorders,
    IReadOnlyList<PhysicalSymptomDto> PhysicalSymptoms
);