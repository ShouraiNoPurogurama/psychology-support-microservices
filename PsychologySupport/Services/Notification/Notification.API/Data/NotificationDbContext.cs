using Microsoft.EntityFrameworkCore;
using Notification.API.Data.Configurations;
using Notification.API.Models;
using Notification.API.Models.Notifications;

namespace Notification.API.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    // Existing entities
    public DbSet<EmailTrace> EmailTraces => Set<EmailTrace>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    
    // New notification entities
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();
    public DbSet<ProcessedIntegrationEvent> ProcessedIntegrationEvents => Set<ProcessedIntegrationEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("public");
        
        // Apply entity configurations
        builder.ApplyConfiguration(new UserNotificationConfiguration());
        builder.ApplyConfiguration(new NotificationPreferencesConfiguration());
        builder.ApplyConfiguration(new ProcessedIntegrationEventConfiguration());
    }
}
