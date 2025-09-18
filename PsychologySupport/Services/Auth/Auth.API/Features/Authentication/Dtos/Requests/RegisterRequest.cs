namespace Auth.API.Features.Authentication.Dtos.Requests;

public record RegisterRequest(
    string FullName,
    string Email,
    string? PhoneNumber,
    string Password,
    string ConfirmPassword
);