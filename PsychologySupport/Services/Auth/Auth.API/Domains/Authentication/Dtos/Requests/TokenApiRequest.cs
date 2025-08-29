namespace Auth.API.Domains.Authentication.Dtos.Requests;

public record TokenApiRequest(string Token, string RefreshToken, string? ClientDeviceId);