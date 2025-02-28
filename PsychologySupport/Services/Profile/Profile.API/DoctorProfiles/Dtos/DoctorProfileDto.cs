using BuildingBlocks.Data.Enums;

namespace Profile.API.DoctorProfiles.Dtos;

public record DoctorProfileDto(
    Guid Id,
    string FullName,
    string Gender,
    ContactInfo ContactInfo,
    string Specialty,
    string Qualifications,
    int YearsOfExperience,
    string Bio,
    float Rating,
    int TotalReviews
);