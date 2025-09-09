using BuildingBlocks.Data.Common;

namespace Profile.API.Domains.Public.DoctorProfiles.Dtos;

public record CreateDoctorProfileDto(
        string FullName,
        UserGender Gender,
        ContactInfo ContactInfo,
        string Qualifications,
        int YearsOfExperience,
        string? Bio
    );