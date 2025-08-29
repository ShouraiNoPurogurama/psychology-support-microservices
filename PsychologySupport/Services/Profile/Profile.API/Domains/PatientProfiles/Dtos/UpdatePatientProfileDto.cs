using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;

namespace Profile.API.Domains.PatientProfiles.Dtos;

public record UpdatePatientProfileDto(
    string? FullName,
    UserGender? Gender,
    string? Allergies,
    PersonalityTrait? PersonalityTraits,
    ContactInfo? ContactInfo,
    Guid? JobId,
    DateOnly? BirthDate
);