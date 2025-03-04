using BuildingBlocks.Enums;
using Profile.API.PatientProfiles.Enum;

namespace Profile.API.PatientProfiles.Dtos;

public record UpdatePatientProfileDto(
    string? FullName,
    UserGender? Gender,
    string? Allergies,
    PersonalityTrait? PersonalityTraits,
    ContactInfo? ContactInfo
);