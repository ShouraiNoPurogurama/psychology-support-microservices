using BuildingBlocks.Pagination;

namespace Profile.API.Domains.PatientProfiles.Dtos;

public record GetCreatedPatientProfileDto(    
    DateTime Date,
    PaginatedResult<SimplifiedPatientProfileDto> Profiles
    );