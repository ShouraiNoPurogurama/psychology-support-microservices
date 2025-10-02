using Auth.API.Data;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using BuildingBlocks.Exceptions;

namespace Auth.API.Features.Authentication.Services.Shared;

public class DeviceManagementService(AuthDbContext authDbContext, ITokenRevocationService tokenRevocationService)
    : IDeviceManagementService
{
    // Trong file DeviceManagementService.cs

// Phương thức mới để gom nhóm các tác vụ
    public async Task<Device> HandleDeviceAndSessionAsync(
        Guid userId,
        string clientDeviceId,
        DeviceType deviceType,
        string? deviceToken,
        string newAccessTokenId,
        string newRefreshToken)
    {
        //1. Upsert device 
        var device = await GetOrUpsertDeviceAsync(userId, clientDeviceId, deviceType, deviceToken);

        //2. Thêm session mới vào context (chưa save)
        var newSession = new DeviceSession
        {
            DeviceId = device.Id,
            AccessTokenId = newAccessTokenId,
            RefreshToken = newRefreshToken
        };
        authDbContext.DeviceSessions.Add(newSession);

        //3. Tìm và đánh dấu các session cũ cần thu hồi (1 DB call)
        int allowedLimit = deviceType switch
        {
            DeviceType.Web => 2,
            _ => 1
        };

        var sessionsToRevoke = await authDbContext.DeviceSessions
            .Where(s => s.Device.UserId == userId && s.Device.DeviceType == deviceType && !s.IsRevoked)
            .OrderByDescending(s => s.CreatedAt)
            .Skip(allowedLimit - 1)
            .ToListAsync();

        foreach (var session in sessionsToRevoke)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTimeOffset.UtcNow;
        }

        await authDbContext.SaveChangesAsync();

        return device;
    }

    public async Task<Device> GetOrUpsertDeviceAsync(
        Guid userId, string clientDeviceId, DeviceType deviceType, string? deviceToken)
    {
        var deviceTypeValue = deviceType.ToString();

        //upsert + trả về bản ghi cuối cùng
        var device = (await authDbContext.Devices
                .FromSqlInterpolated($@"
        INSERT INTO devices (id, user_id, client_device_id, device_type, device_token, last_used_at)
        VALUES ({Guid.NewGuid()}, {userId}, {clientDeviceId}, {deviceType.ToString()}, {deviceToken}, now())
        ON CONFLICT (client_device_id, user_id) DO UPDATE
        SET  device_type  = EXCLUDED.device_type,
             device_token = COALESCE(EXCLUDED.device_token, devices.device_token),
             last_used_at = now()
        RETURNING *")
                .AsNoTracking()
                .ToListAsync())
                .Single();

        return device;
    }


    public async Task<List<DeviceSession>> GetOldestSessionsToRevokeAsync(Guid userId, DeviceType deviceType, Guid deviceId,
        string currentJti)
    {
        int allowedLimit = deviceType switch
        {
            DeviceType.Web => 2,
            DeviceType.IOS => 1,
            DeviceType.Android => 1,
            _ => 1
        };

        var sessionsToRevoke = await authDbContext.DeviceSessions
            .Where(s => s.Device.UserId == userId && s.Device.DeviceType == deviceType && !s.IsRevoked)
            .OrderByDescending(s => s.CreatedAt)
            .Skip(allowedLimit - 1)
            .ToListAsync();

        return sessionsToRevoke;
    }
    
    public void MarkSessionsAsRevoked(IEnumerable<DeviceSession> sessionsToRevoke)
    {
        foreach (var session in sessionsToRevoke)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTimeOffset.UtcNow;
        }
    }

    public DeviceSession PrepareNewDeviceSession(Guid deviceId, string accessTokenId, string refreshToken)
    {
        var session = new DeviceSession
        {
            DeviceId = deviceId,
            AccessTokenId = accessTokenId,
            RefreshToken = refreshToken
        };

        authDbContext.DeviceSessions.Add(session);

        return session;
    }
}