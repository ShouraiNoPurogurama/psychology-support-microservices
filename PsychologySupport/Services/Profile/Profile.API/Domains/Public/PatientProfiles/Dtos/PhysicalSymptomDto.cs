namespace Profile.API.Domains.Public.PatientProfiles.Dtos;

public record PhysicalSymptomDto(
    Guid Id,
    string Name,
    string Description
);