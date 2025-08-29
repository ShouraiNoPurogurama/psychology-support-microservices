using BuildingBlocks.Enums;

namespace Profile.API.Domains.PatientProfiles.Dtos;

public record SimplifiedPatientProfileDto(    
    Guid Id,
    string FullName,
    UserGender Gender,
    DateOnly? BirthDate,
    DateTimeOffset? CreatedAt);