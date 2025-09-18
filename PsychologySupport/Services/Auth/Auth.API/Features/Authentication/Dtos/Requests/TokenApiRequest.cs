namespace Auth.API.Features.Authentication.Dtos.Requests;

public record TokenApiRequest(string Token, string RefreshToken, string? ClientDeviceId);