using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;
using Profile.API.DoctorProfiles.Models;

namespace Profile.API.DoctorProfiles.Dtos;

public record CreateDoctorProfileDto(
        string FullName,
        UserGender Gender,
        ContactInfo ContactInfo,
        string Qualifications,
        int YearsOfExperience,
        string? Bio
    );