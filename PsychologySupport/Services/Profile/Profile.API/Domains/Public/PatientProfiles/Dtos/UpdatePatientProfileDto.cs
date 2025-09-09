namespace Profile.API.Domains.PatientProfiles.Dtos;

public record UpdatePatientProfileDto(
    UserGender? Gender,
    string? Allergies,
    PersonalityTrait? PersonalityTraits,
    Guid? JobId
);