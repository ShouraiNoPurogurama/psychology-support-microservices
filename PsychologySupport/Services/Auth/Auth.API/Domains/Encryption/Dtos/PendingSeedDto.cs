using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;

namespace Auth.API.Domains.Encryption.Dtos;

public record PendingSeedDto(
    string FullName,
    UserGender Gender,
    DateOnly BirthDate,
    ContactInfo? ContactInfo
);