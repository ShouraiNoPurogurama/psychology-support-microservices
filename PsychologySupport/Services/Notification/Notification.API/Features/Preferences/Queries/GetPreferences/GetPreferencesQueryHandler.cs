using BuildingBlocks.CQRS;
using Notification.API.Abstractions;

namespace Notification.API.Features.Preferences.Queries.GetPreferences;

public class GetPreferencesQueryHandler : IQueryHandler<GetPreferencesQuery, GetPreferencesResult>
{
    private readonly IPreferencesCache _preferencesCache;

    public GetPreferencesQueryHandler(IPreferencesCache preferencesCache)
    {
        _preferencesCache = preferencesCache;
    }

    public async Task<GetPreferencesResult> Handle(GetPreferencesQuery request, CancellationToken cancellationToken)
    {
        var preferences = await _preferencesCache.GetOrDefaultAsync(request.AliasId, cancellationToken);

        return new GetPreferencesResult(
            preferences.Id,
            preferences.ReactionsEnabled,
            preferences.CommentsEnabled,
            preferences.MentionsEnabled,
            preferences.FollowsEnabled,
            preferences.ModerationEnabled,
            preferences.BotEnabled,
            preferences.SystemEnabled
        );
    }
}
