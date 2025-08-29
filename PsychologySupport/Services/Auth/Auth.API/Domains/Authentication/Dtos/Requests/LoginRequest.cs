namespace Auth.API.Domains.Authentication.Dtos.Requests;

public record LoginRequest(string? Email, string? PhoneNumber, string Password, string? DeviceToken, DeviceType? DeviceType, string? ClientDeviceId);