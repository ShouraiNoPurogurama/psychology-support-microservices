using Auth.API.Data;
using Auth.API.Domains.Authentication.ServiceContracts.Shared;

namespace Auth.API.Domains.Authentication.Services.Shared;

public class DeviceManagementService(AuthDbContext authDbContext) : IDeviceManagementService
{

    public async Task<Device> GetOrUpsertDeviceAsync(Guid userId, string clientDeviceId, DeviceType deviceType,
        string? deviceToken)
    {
        var device = await authDbContext.Devices.FirstOrDefaultAsync(d =>
            d.ClientDeviceId == clientDeviceId && d.UserId == userId);

        if (device is null)
        {
            device = new Device
            {
                UserId = userId,
                ClientDeviceId = clientDeviceId,
                DeviceType = deviceType,
                DeviceToken = deviceToken,
                LastUsedAt = DateTime.UtcNow
            };
            authDbContext.Devices.Add(device);
        }
        else
        {
            device.ClientDeviceId = clientDeviceId;
            device.DeviceToken = deviceToken;
            device.LastUsedAt = DateTime.UtcNow;
            device.DeviceType = deviceType;
            authDbContext.Devices.Update(device);
        }

        await authDbContext.SaveChangesAsync();
        return device;
    }

    public async Task ManageDeviceSessionsAsync(Guid userId, DeviceType deviceType, Guid deviceId)
    {
        int allowedLimit = deviceType switch
        {
            DeviceType.Web => 2,
            DeviceType.IOS => 1,
            DeviceType.Android => 1,
            _ => 1
        };

        var allDeviceIds = await authDbContext.Devices
            .Where(d => d.UserId == userId && d.DeviceType == deviceType)
            .Select(d => d.Id)
            .ToListAsync();

        var activeSessions = await authDbContext.DeviceSessions
            .Where(s => allDeviceIds.Contains(s.DeviceId) && !s.IsRevoked)
            .OrderBy(s => s.LastRefeshToken ?? s.CreatedAt)
            .ToListAsync();

        if (activeSessions.Count >= allowedLimit)
        {
            var oldestSession = activeSessions.First();
            oldestSession.IsRevoked = true;
            oldestSession.RevokedAt = DateTimeOffset.UtcNow;
        }
    }
}