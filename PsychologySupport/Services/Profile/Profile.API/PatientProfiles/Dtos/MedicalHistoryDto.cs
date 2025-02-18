using Profile.API.MentalDisorders.Models;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Dtos;

public record MedicalHistoryDto(
    string Description,
    DateTimeOffset DiagnosedAt,
    IReadOnlyList<SpecificMentalDisorder> SpecificMentalDisorders,
    IReadOnlyList<PhysicalSymptom> PhysicalSymptoms
);