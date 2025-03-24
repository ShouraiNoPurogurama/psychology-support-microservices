using ChatBox.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Data;

public class ChatBoxDbContext : DbContext
{
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<DoctorPatientBooking> DoctorPatients => Set<DoctorPatientBooking>();
    
    public ChatBoxDbContext(DbContextOptions<ChatBoxDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("public");
        
        builder.Entity<DoctorPatientBooking>()
            .HasKey(d => d.BookingId);
    }
}