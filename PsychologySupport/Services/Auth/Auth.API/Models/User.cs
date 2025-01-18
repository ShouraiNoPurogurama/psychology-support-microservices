using Microsoft.AspNetCore.Identity;

namespace Auth.API.Models;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = default!;
    public string Gender { get; set; } = default!;
}