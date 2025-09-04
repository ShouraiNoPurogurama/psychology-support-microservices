using System.Reflection;
using BuildingBlocks.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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
    public DbSet<PendingVerificationUser> PendingVerificationUsers { get; set; }

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
        
        builder.Entity<PendingVerificationUser>(e =>
        {
            e.Property(x => x.PayloadProtected).IsRequired(); // bytea
            e.Ignore(x => x.FullName);
            e.Ignore(x => x.Gender);
            e.Ignore(x => x.BirthDate);
            e.Ignore(x => x.ContactInfo);
            
            e.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<PendingVerificationUser>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
        });
    }
}