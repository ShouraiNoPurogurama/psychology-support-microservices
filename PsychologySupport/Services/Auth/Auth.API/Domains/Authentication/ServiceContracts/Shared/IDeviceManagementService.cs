namespace Auth.API.Domains.Authentication.ServiceContracts.Shared;

public interface IDeviceManagementService
{
    Task<Device> GetOrUpsertDeviceAsync(Guid userId, string clientDeviceId, DeviceType deviceType,
        string? deviceToken);
    Task ManageDeviceSessionsAsync(Guid userId, DeviceType deviceType, Guid deviceId);
}