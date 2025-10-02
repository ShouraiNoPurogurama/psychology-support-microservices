namespace Auth.API.Features.Authentication.Dtos.Responses;

public record OnboardingStatusDto(bool PiiCompleted, bool PatientProfileCompleted, bool AliasIssued);