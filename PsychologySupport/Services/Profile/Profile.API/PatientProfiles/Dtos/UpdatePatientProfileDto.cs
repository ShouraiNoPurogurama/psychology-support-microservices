using Profile.API.PatientProfiles.ValueObjects;

namespace Profile.API.PatientProfiles.Dtos;

public record UpdatePatientProfileDto(
    string FullName,
    string Gender,
    string Allergies,
    PersonalityTrait PersonalityTraits,
    ContactInfo ContactInfo
);