using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;
using Profile.API.PatientProfiles.Enum;

namespace Profile.API.PatientProfiles.Dtos;

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