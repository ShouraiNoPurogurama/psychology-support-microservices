using BuildingBlocks.Data.Common;
using BuildingBlocks.Enums;

namespace Profile.API.Domains.Pii.Dtos;

public record PersonSeed(string? FullName, UserGender? Gender, DateOnly? BirthDate, ContactInfo? ContactInfo);