namespace Profile.API.DoctorProfiles.Dtos;

public record DoctorProfileDto(
    Guid Id,
    string? Specialty,
    string? Qualifications,
    int? YearsOfExperience,
    string? Bio,
    string? ProfilePicture
);