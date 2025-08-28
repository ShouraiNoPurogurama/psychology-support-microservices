using BuildingBlocks.Enums;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.Models;

public class User : IdentityUser<Guid>
{
    public string? FirebaseUserId { get; set; }
    public string FullName { get; set; } = default!;
    public UserGender Gender { get; set; } = default!;
    
    public DateOnly? BirthDate { get; set; }
    
    public virtual ICollection<Device> Devices { get; set; } = [];
}