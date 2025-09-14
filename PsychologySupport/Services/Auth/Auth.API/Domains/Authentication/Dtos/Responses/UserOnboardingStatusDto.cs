using Auth.API.Enums;

namespace Auth.API.Domains.Authentication.Dtos.Responses;

public record UserOnboardingStatusDto(UserOnboardingStatus Status, bool PiiCompleted, bool PatientProfileCompleted);