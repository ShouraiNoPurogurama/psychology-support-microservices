namespace Auth.API.Models
{
    public class DeviceSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DeviceId { get; set; }             
        public string AccessTokenId { get; set; }  
        public string RefreshToken { get; set; } = default!;
        public bool IsRevoked { get; set; } = false;
        public DateTimeOffset? RevokedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? LastRefeshToken { get; set; }
        public virtual Device Device { get; set; }
    }
}
