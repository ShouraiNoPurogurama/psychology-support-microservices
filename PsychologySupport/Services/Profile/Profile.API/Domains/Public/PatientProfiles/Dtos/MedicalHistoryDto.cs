using Profile.API.Domains.Public.MentalDisorders.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Dtos;

public record MedicalHistoryDto(
    string Description,
    DateTimeOffset DiagnosedAt,
    IReadOnlyList<SpecificMentalDisorderDto> SpecificMentalDisorders,
    IReadOnlyList<PhysicalSymptomDto> PhysicalSymptoms
);