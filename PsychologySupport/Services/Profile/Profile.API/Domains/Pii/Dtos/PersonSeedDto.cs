using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;

namespace Profile.API.Domains.Pii.Dtos;

public record PersonSeedDto(string FullName, UserGender Gender, DateOnly BirthDate, ContactInfo ContactInfo);