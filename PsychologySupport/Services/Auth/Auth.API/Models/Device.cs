namespace Auth.API.Models;

public class Device
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string?  DeviceToken { get; set; }  // FCM firebase token for push notifications
    public DeviceType DeviceType { get; set; }
    public string ClientDeviceId { get; set; } = default!;
    public DateTimeOffset LastUsedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual User User { get; set; }
    public virtual ICollection<DeviceSession> DeviceSessions { get; set; } = new List<DeviceSession>();
}