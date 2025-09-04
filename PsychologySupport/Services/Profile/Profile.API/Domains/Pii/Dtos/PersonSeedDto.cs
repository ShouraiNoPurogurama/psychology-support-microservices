using BuildingBlocks.Data.Common;

namespace Profile.API.Domains.Pii.Dtos;

public record PersonSeedDto(string FullName, UserGender Gender, DateOnly BirthDate, ContactInfo ContactInfo);