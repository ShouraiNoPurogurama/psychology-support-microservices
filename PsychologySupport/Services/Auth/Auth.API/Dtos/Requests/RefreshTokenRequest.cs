namespace Auth.API.Dtos.Requests;

public record RefreshTokenRequest(string JwtToken, string RefreshToken);