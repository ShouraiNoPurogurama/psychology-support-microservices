using System.Reflection;
using System.Text.Json;
using Auth.API.Enums;
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
    
    public DbSet<UserOnboarding> UserOnboardings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        Console.WriteLine($"***************** Executing assembly: {Assembly.GetExecutingAssembly().FullName} **************");
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.Property(u => u.OnboardingStatus)
                .HasConversion(s => s.ToString(),
                    dbStatus => (UserOnboardingStatus)Enum.Parse(typeof(UserOnboardingStatus), dbStatus))
                .HasSentinel(UserOnboardingStatus.Pending)
                .HasDefaultValue(UserOnboardingStatus.Pending);
        });
        
        builder.Entity<UserOnboarding>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.UserId).IsUnique();

            entity.HasOne(x => x.User)
                .WithOne(u => u.Onboarding)
                .HasForeignKey<UserOnboarding>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(x => x.Status)
                .HasConversion(s => s.ToString(),
                    dbStatus => (UserOnboardingStatus)Enum.Parse(typeof(UserOnboardingStatus), dbStatus));

            //Missing[] lưu JSON (Postgres: jsonb; SQL Server: nvarchar(max))
            entity.Property(x => x.Missing)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<string>()
                )
                .HasColumnType("jsonb");
        });

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

            e.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<PendingVerificationUser>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}