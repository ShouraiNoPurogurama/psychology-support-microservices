using BuildingBlocks.Data.Common;

namespace Profile.API.Domains.Pii.Dtos;

public record PersonSeedDto(Guid SubjectRef, string FullName, UserGender Gender, DateOnly BirthDate, ContactInfo ContactInfo);