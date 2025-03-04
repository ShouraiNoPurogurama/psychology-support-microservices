using BuildingBlocks.Enums;
using Profile.API.PatientProfiles.Enum;

namespace Profile.API.PatientProfiles.Dtos;

public record CreatePatientProfileDto(
    Guid UserId,
    string FullName,
    UserGender Gender,
    string? Allergies,
    PersonalityTrait PersonalityTraits,
    ContactInfo ContactInfo
);