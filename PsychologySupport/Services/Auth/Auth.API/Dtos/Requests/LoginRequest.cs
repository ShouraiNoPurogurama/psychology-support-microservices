namespace Auth.API.Dtos.Requests;

public record LoginRequest(string? Email, string? PhoneNumber, string Password);