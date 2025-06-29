namespace Auth.API.Dtos.Requests;

public record TokenApiRequest(string Token, string RefreshToken, string? ClientDeviceId);