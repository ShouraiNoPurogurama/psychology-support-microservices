namespace Auth.API.Features.Authentication.Dtos.Requests;

public record LoginRequest(
    string? Email,
    string? PhoneNumber,
    string Password,
    string? DeviceToken,
    DeviceType? DeviceType,
    string? ClientDeviceId);