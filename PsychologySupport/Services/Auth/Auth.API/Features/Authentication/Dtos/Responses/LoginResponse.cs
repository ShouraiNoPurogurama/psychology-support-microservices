namespace Auth.API.Features.Authentication.Dtos.Responses;

public record LoginResponse(string Token, string RefreshToken);