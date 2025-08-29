using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;

namespace Profile.API.Domains.DoctorProfiles.Dtos;

public record DoctorProfileDto(
    Guid Id,
    Guid UserId,
    string FullName,
    UserGender Gender,
    ContactInfo ContactInfo,
    List<SpecialtyDto> Specialties,
    string Qualifications,
    int YearsOfExperience,
    string Bio,
    float Rating
);