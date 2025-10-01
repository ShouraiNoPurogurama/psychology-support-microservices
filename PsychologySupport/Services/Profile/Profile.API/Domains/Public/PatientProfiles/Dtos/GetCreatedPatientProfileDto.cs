using BuildingBlocks.Pagination;

namespace Profile.API.Domains.Public.PatientProfiles.Dtos;

public record GetCreatedPatientProfileDto(    
    DateTimeOffset Date,
    PaginatedResult<SimplifiedPatientProfileDto> Profiles
    );