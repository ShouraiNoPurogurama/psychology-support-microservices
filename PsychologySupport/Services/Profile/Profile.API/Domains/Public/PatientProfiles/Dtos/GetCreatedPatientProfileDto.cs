using BuildingBlocks.Pagination;

namespace Profile.API.Domains.Public.PatientProfiles.Dtos;

public record GetCreatedPatientProfileDto(    
    DateTime Date,
    PaginatedResult<SimplifiedPatientProfileDto> Profiles
    );