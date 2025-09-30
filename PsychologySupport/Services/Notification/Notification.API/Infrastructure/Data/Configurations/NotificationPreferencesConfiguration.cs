using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.API.Features.Preferences.Models;

namespace Notification.API.Infrastructure.Data.Configurations;

public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.ToTable("notification_preferences");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.ReactionsEnabled).IsRequired();
        builder.Property(p => p.CommentsEnabled).IsRequired();
        builder.Property(p => p.MentionsEnabled).IsRequired();
        builder.Property(p => p.FollowsEnabled).IsRequired();
        builder.Property(p => p.ModerationEnabled).IsRequired();
        builder.Property(p => p.BotEnabled).IsRequired();
        builder.Property(p => p.SystemEnabled).IsRequired();
    }
}
