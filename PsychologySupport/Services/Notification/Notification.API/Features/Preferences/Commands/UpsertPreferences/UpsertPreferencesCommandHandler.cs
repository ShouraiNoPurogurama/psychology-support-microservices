using BuildingBlocks.CQRS;
using Notification.API.Contracts;
using Notification.API.Features.Preferences.Models;

namespace Notification.API.Features.Preferences.Commands.UpsertPreferences;

public class UpsertPreferencesCommandHandler : ICommandHandler<UpsertPreferencesCommand, UpsertPreferencesResult>
{
    private readonly INotificationPreferencesRepository _repository;
    private readonly IPreferencesCache _cache;

    public UpsertPreferencesCommandHandler(
        INotificationPreferencesRepository repository,
        IPreferencesCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<UpsertPreferencesResult> Handle(UpsertPreferencesCommand request, CancellationToken cancellationToken)
    {
        var preferences = new NotificationPreferences
        {
            Id = request.AliasId,
            ReactionsEnabled = request.ReactionsEnabled,
            CommentsEnabled = request.CommentsEnabled,
            MentionsEnabled = request.MentionsEnabled,
            FollowsEnabled = request.FollowsEnabled,
            ModerationEnabled = request.ModerationEnabled,
            BotEnabled = request.BotEnabled,
            SystemEnabled = request.SystemEnabled,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        await _repository.UpsertAsync(preferences, cancellationToken);

        // Clear cache to force refresh
        _cache.Remove(request.AliasId);

        return new UpsertPreferencesResult(true);
    }
}
