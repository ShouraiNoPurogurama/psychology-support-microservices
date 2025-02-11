namespace Profile.API.Dtos;

public record DoctorProfileDto(
    Guid Id,
    string? Specialty,
    string? Qualifications,
    int? YearsOfExperience,
    string? Bio,
    string? ProfilePicture
);