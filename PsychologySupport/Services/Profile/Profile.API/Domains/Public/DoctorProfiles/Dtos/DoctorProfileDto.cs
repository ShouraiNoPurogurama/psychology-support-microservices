using BuildingBlocks.Data.Common;

namespace Profile.API.Domains.Public.DoctorProfiles.Dtos;

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