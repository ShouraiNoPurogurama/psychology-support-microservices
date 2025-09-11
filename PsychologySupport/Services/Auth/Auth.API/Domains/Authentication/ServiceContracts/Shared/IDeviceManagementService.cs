namespace Auth.API.Domains.Authentication.ServiceContracts.Shared;

public interface IDeviceManagementService
{
    Task<Device> GetOrUpsertDeviceAsync(Guid userId, string clientDeviceId, DeviceType deviceType,
        string? deviceToken);
    Task RevokeOldestSessionIfLimitExceededAsync(Guid userId, DeviceType deviceType, Guid deviceId);
    Task<DeviceSession> CreateDeviceSessionAsync(Guid deviceId, string accessTokenId, string refreshToken);
}