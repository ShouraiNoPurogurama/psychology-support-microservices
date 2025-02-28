namespace Profile.API.DoctorProfiles.Dtos;

public record DoctorProfileDto(
    string FullName,
    string Gender,
    ContactInfo ContactInfo,
    List<SpecialtyDto> Specialties,
    string Qualifications,
    int YearsOfExperience,
    string Bio
);