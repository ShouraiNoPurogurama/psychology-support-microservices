using Microsoft.EntityFrameworkCore;
using Notification.API.Contracts;
using Notification.API.Data;
using Notification.API.Features.Notifications.Models;
using Notification.API.Features.Preferences.Models;
using Notification.API.Infrastructure.Data;

namespace Notification.API.Infrastructure.Repositories;

public class NotificationPreferencesRepository : INotificationPreferencesRepository
{
    private readonly NotificationDbContext _context;

    public NotificationPreferencesRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreferences?> GetByIdAsync(Guid aliasId, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.Id == aliasId, cancellationToken);
    }

    public async Task<NotificationPreferences> GetOrDefaultAsync(Guid aliasId, CancellationToken cancellationToken = default)
    {
        var preferences = await GetByIdAsync(aliasId, cancellationToken);
        return preferences ?? NotificationPreferences.CreateDefault(aliasId);
    }

    public async Task<List<NotificationPreferences>> GetOrDefaultAsync(List<Guid> aliasId, CancellationToken cancellationToken = default)
    {
        var preferences = await _context.NotificationPreferences
            .Where(p => aliasId.Contains(p.Id))
            .ToListAsync(cancellationToken);

        return preferences;
    }


    public async Task UpsertAsync(NotificationPreferences preferences, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(preferences.Id, cancellationToken);
        
        if (existing == null)
        {
            await _context.NotificationPreferences.AddAsync(preferences, cancellationToken);
        }
        else
        {
            existing.ReactionsEnabled = preferences.ReactionsEnabled;
            existing.CommentsEnabled = preferences.CommentsEnabled;
            existing.MentionsEnabled = preferences.MentionsEnabled;
            existing.FollowsEnabled = preferences.FollowsEnabled;
            existing.ModerationEnabled = preferences.ModerationEnabled;
            existing.BotEnabled = preferences.BotEnabled;
            existing.SystemEnabled = preferences.SystemEnabled;
            existing.LastModified = DateTimeOffset.UtcNow;
            
            _context.NotificationPreferences.Update(existing);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
