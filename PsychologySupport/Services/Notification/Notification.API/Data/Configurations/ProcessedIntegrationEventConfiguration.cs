using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.API.Models.Notifications;

namespace Notification.API.Data.Configurations;

public class ProcessedIntegrationEventConfiguration : IEntityTypeConfiguration<ProcessedIntegrationEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedIntegrationEvent> builder)
    {
        builder.ToTable("processed_integration_events");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(255);
        builder.Property(e => e.ReceivedAt).IsRequired();
        
        // Index for cleanup queries
        builder.HasIndex(e => e.ReceivedAt)
            .HasDatabaseName("idx_processed_events_received");
    }
}
