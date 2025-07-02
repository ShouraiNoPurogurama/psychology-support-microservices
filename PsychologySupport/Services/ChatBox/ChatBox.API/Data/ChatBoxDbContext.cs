using ChatBox.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Data;

public class ChatBoxDbContext : DbContext
{
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<DoctorPatientBooking> DoctorPatients => Set<DoctorPatientBooking>();
    public DbSet<AIMessage> AIChatMessages => Set<AIMessage>();
    public DbSet<AIChatSession> AIChatSessions => Set<AIChatSession>();
    
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
    }
}