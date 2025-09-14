namespace Profile.API.Domains.Pii.Dtos;

public record PersonSeedDto(Guid SubjectRef, Guid PatientProfileId, string FullName, string Email, string? PhoneNumber = null);