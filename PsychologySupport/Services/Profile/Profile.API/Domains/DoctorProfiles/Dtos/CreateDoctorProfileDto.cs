using BuildingBlocks.Data.Common;

namespace Profile.API.Domains.DoctorProfiles.Dtos;

public record CreateDoctorProfileDto(
        string FullName,
        UserGender Gender,
        ContactInfo ContactInfo,
        string Qualifications,
        int YearsOfExperience,
        string? Bio
    );