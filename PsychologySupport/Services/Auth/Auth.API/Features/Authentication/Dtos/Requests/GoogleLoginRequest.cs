namespace Auth.API.Features.Authentication.Dtos.Requests;

public record GoogleLoginRequest
{
    public string GoogleIdToken { get; set; } = string.Empty;
    public string? DeviceToken { get; set; }
    public DeviceType? DeviceType { get; set; }
    public string? ClientDeviceId { get; set; }
    public string? ReferralCode { get; set; }
}