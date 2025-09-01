using Microsoft.AspNetCore.Identity;

namespace Auth.API.Models;

public class User : IdentityUser<Guid>
{
    public string? FirebaseUserId { get; set; }
    public virtual ICollection<Device> Devices { get; set; } = [];
}