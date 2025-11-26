using ChatBox.API.Models;
using ChatBox.API.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Data;

public class ChatBoxDbContext : DbContext
{
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<DoctorPatientBooking> DoctorPatients => Set<DoctorPatientBooking>();
    public DbSet<AIMessage> AIChatMessages => Set<AIMessage>();
    public DbSet<AIChatSession> AIChatSessions => Set<AIChatSession>();
    public DbSet<PendingStickerReward> PendingStickerRewards => Set<PendingStickerReward>();
    public DbSet<UserOnScreenStat> UserOnScreenStats => Set<UserOnScreenStat>();
    
    public ChatBoxDbContext(DbContextOptions<ChatBoxDbContext> options) : base(options)
    {
    }

    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("public");
        
        builder.Entity<DoctorPatientBooking>()
            .HasKey(d => d.BookingId);
        
        builder.Entity<AIChatSession>()
            .Ignore(e => e.PersonaSnapshot) //Không map object
            .Property(e => e.PersonaSnapshotJson) //Map field chứa JSON string
            .HasColumnName("PersonaSnapshot")
            .HasColumnType("jsonb"); //Nếu là PostgreSQL, còn SQL Server thì nvarchar(max)

        builder.Entity<AIChatSession>(entity =>
        {
            entity.Property(e => e.IsLegacy)
                .HasDefaultValue(true);
        });
        
        builder.Entity<PendingStickerReward>() 
            .HasKey(e => e.RewardId);

        builder.Entity<UserOnScreenStat>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("view_user_onscreen_stats");
        });
    }
}