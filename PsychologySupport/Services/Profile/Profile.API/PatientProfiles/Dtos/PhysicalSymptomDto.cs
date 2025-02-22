namespace Profile.API.PatientProfiles.Dtos;

public record PhysicalSymptomDto(
    Guid Id,
    string Name,
    string Description
);
