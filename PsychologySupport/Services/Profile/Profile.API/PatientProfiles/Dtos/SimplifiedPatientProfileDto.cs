using BuildingBlocks.Enums;

namespace Profile.API.PatientProfiles.Dtos;

public record SimplifiedPatientProfileDto(    
    Guid Id,
    string FullName,
    UserGender Gender,
    DateOnly? BirthDate,
    DateTimeOffset? CreatedAt);