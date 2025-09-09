using BuildingBlocks.Data.Common;

namespace Profile.API.Domains.Public.PatientProfiles.Dtos;

public record CreatePatientProfileDto(
    Guid AliasId,
    string FullName,
    UserGender Gender,
    string? Allergies,
    PersonalityTrait PersonalityTraits,
    ContactInfo ContactInfo,
    Guid? JobId,
    DateOnly? BirthDate
);