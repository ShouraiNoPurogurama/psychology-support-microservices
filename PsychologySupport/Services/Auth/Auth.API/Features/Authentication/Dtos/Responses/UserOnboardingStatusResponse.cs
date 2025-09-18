using Auth.API.Enums;

namespace Auth.API.Features.Authentication.Dtos.Responses;

public record UserOnboardingStatusResponse(UserOnboardingStatus Status, bool PiiCompleted, bool PatientProfileCompleted);