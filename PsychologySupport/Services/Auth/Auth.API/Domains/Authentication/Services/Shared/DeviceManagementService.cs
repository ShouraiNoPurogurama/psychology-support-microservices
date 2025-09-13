using System.Runtime.InteropServices;
using Auth.API.Data;
using Auth.API.Domains.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Exceptions;

namespace Auth.API.Domains.Authentication.Services.Shared;

public class DeviceManagementService(AuthDbContext authDbContext, ITokenRevocationService tokenRevocationService) : IDeviceManagementService
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
        }

        await authDbContext.SaveChangesAsync();
        return device;
    }
    

    public async Task RevokeOldestSessionIfLimitExceededAsync(Guid userId, DeviceType deviceType, Guid deviceId, string currentJti)
    {
        int allowedLimit = deviceType switch
        {
            DeviceType.Web => 2,
            DeviceType.IOS => 1,
            DeviceType.Android => 1,
            _ => 1
        };

        var userDeviceIds = await authDbContext.Devices.AsNoTracking()
            .Where(d => d.UserId == userId &&  d.DeviceType == deviceType)
            .Select(d => d.Id)
            .ToListAsync();

        if (!userDeviceIds.Any()) return;

        var activeSessions = await authDbContext.DeviceSessions
            .Where(s => userDeviceIds.Contains(s.DeviceId) && !s.IsRevoked && s.AccessTokenId != currentJti.ToString())
            .OrderBy(s => s.CreatedAt) 
            .ToListAsync();

        if (activeSessions.Count > allowedLimit)
        {
            int sessionsToRevokeCount = activeSessions.Count - allowedLimit;
            var sessionsToRevoke = activeSessions.Take(sessionsToRevokeCount);

            await tokenRevocationService.RevokeSessionsAsync(sessionsToRevoke);
        }
    }

    public async Task<DeviceSession> CreateDeviceSessionAsync(Guid deviceId, string accessTokenId, string refreshToken)
    {
        var existingDevice = await authDbContext.Devices
            .AnyAsync(d => d.Id == deviceId);
        if (!existingDevice)
            throw new BadRequestException("Thiết bị không hợp lệ.");

        var session = new DeviceSession
        {
            DeviceId = deviceId,
            AccessTokenId = accessTokenId,
            RefreshToken = refreshToken
        };

        authDbContext.DeviceSessions.Add(session);
        await authDbContext.SaveChangesAsync();

        return session;
    }
}