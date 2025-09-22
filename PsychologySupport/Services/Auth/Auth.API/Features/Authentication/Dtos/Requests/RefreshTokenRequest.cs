namespace Auth.API.Features.Authentication.Dtos.Requests;

public record RefreshTokenRequest(string JwtToken, string RefreshToken);