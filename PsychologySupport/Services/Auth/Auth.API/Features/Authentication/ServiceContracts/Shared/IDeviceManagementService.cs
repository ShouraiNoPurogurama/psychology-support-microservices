namespace Auth.API.Features.Authentication.ServiceContracts.Shared;

public interface IDeviceManagementService
{
    Task<Device> GetOrUpsertDeviceAsync(Guid userId, string clientDeviceId, DeviceType deviceType,
        string? deviceToken);
    Task<List<DeviceSession>> GetOldestSessionsToRevokeAsync(Guid userId, DeviceType deviceType, Guid deviceId, string currentJti);
    void MarkSessionsAsRevoked(IEnumerable<DeviceSession> sessionsToRevoke);
    DeviceSession PrepareNewDeviceSession(Guid deviceId, string accessTokenId, string refreshToken);
}