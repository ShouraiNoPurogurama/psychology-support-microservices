namespace Profile.API.Domains.Public.PatientProfiles.Dtos;

public record PatchPatientProfileDto(
    UserGender? Gender,
    string? Allergies,
    PersonalityTrait? PersonalityTraits,
    Guid? JobId
);