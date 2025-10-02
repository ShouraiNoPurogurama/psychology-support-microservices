using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.API.Features.Notifications.Models;

namespace Notification.API.Infrastructure.Data.Configurations;

public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable("user_notifications");
        
        builder.HasKey(n => n.Id);
        
        builder.Property(n => n.RecipientAliasId).IsRequired();
        builder.Property(n => n.ActorDisplayName).IsRequired().HasMaxLength(255);
        builder.Property(n => n.Type).IsRequired();
        builder.Property(n => n.IsRead).IsRequired();
        builder.Property(n => n.DedupeHash).HasMaxLength(64);
        builder.Property(n => n.GroupingKey).HasMaxLength(255);
        builder.Property(n => n.ModerationAction).HasMaxLength(100);
        builder.Property(n => n.Snippet).HasMaxLength(500);
        
        // Indexes for performance
        builder.HasIndex(n => new { n.RecipientAliasId, n.CreatedAt })
            .HasDatabaseName("idx_user_notifications_recipient_created");
        
        builder.HasIndex(n => new { n.RecipientAliasId, n.IsRead, n.CreatedAt })
            .HasDatabaseName("idx_user_notifications_recipient_unread")
            .HasFilter("is_read = false");
        
        builder.HasIndex(n => new { n.RecipientAliasId, n.Type, n.CreatedAt })
            .HasDatabaseName("idx_user_notifications_recipient_type");
        
        builder.HasIndex(n => new { n.GroupingKey, n.RecipientAliasId, n.IsRead })
            .HasDatabaseName("idx_user_notifications_grouping");
        
        builder.HasIndex(n => n.DedupeHash)
            .HasDatabaseName("idx_user_notifications_dedupe");
    }
}
