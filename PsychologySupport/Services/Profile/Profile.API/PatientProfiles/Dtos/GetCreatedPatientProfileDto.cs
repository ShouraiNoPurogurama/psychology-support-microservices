using BuildingBlocks.Pagination;

namespace Profile.API.PatientProfiles.Dtos;

public record GetCreatedPatientProfileDto(    
    DateTime Date,
    PaginatedResult<SimplifiedPatientProfileDto> Profiles
    );