namespace Profile.API.Domains.Public.PatientProfiles.Dtos;

public record SimplifiedPatientProfileDto(    
    Guid Id,
    string FullName,
    UserGender Gender,
    DateOnly? BirthDate,
    DateTimeOffset? CreatedAt);