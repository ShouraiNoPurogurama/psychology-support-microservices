namespace Auth.API.Models;

public class User : IdentityUser<Guid>
{
    public bool PiiCreated { get; set; } = false;
    public string? FirebaseUserId { get; set; }
    public virtual ICollection<Device> Devices { get; set; } = [];
}