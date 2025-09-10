using BuildingBlocks.Data.Common;

namespace Profile.API.Domains.Pii.Dtos;

public record UpdatePiiDto(
    string? FullName,
    UserGender? Gender,
    ContactInfo? ContactInfo,
    DateOnly? BirthDate
);