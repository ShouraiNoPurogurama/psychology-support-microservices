namespace Auth.API.Domains.Authentication.Dtos.Requests;

public record RefreshTokenRequest(string JwtToken, string RefreshToken);