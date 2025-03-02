using BuildingBlocks.Enums;

namespace Profile.API.DoctorProfiles.Dtos;

public record UpdateDoctorProfileDto(
    string? FullName,
    UserGender? Gender,
    ContactInfo? ContactInfo,
    List<Guid>? SpecialtyIds,
    string? Qualifications,
    int? YearsOfExperience,
    string? Bio
);