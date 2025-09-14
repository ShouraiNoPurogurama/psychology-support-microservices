namespace Profile.API.Domains.Pii.Dtos;

public record PersonSeedDto(Guid SubjectRef, string FullName, string Email, string? PhoneNumber = null);