using BuildingBlocks.Data.Enums;

namespace Profile.API.DoctorProfiles.Dtos;

public record DoctorProfileDto(
    string FullName,
    UserGender Gender,
    ContactInfo ContactInfo,
    List<SpecialtyDto> Specialties,
    string Qualifications,
    int YearsOfExperience,
    string Bio
);