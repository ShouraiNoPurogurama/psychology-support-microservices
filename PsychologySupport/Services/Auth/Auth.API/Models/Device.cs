namespace Auth.API.Models;

public class Device
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public string DeviceToken { get; set; }
    public DeviceType DeviceType { get; set; }
    
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; }
}