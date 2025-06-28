using System.Reflection;
using Auth.API.Models;
using BuildingBlocks.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Data;

public class AuthDbContext : IdentityDbContext<User, Role, Guid>
{
    public AuthDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceSession> DeviceSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        Console.WriteLine($"***************** Executing assembly: {Assembly.GetExecutingAssembly().FullName} **************");
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);

        builder.Entity<Device>(e =>
        {
            e.Property(d => d.DeviceType)
                .HasConversion(d => d.ToString(),
                    dbStatus => (DeviceType)Enum.Parse(typeof(DeviceType), dbStatus));
        });

        builder.Entity<User>(e =>
        {
            e.Property(u => u.Gender)
                .HasConversion(u => u.ToString(),
                    dbStatus => (UserGender)Enum.Parse(typeof(UserGender), dbStatus));
        });
    }
}