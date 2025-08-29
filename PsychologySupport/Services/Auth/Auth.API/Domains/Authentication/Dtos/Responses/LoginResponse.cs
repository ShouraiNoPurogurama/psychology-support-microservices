namespace Auth.API.Domains.Authentication.Dtos.Responses;

public record LoginResponse(string Token, string RefreshToken);