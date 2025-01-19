namespace Auth.API.Dtos.Responses;

public record LoginResponse(string Token, string RefreshToken);