namespace Auth.API.Features.Authentication.Dtos.Requests;

public record ResetPasswordRequest(string Email, string Token, string NewPassword, string ConfirmPassword);