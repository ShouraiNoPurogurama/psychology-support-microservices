using BuildingBlocks.CQRS;

namespace Notification.API.Features.Preferences.Queries.GetPreferences;

public record GetPreferencesQuery(Guid AliasId) : IQuery<GetPreferencesResult>;

public record GetPreferencesResult(
    Guid AliasId,
    bool ReactionsEnabled,
    bool CommentsEnabled,
    bool MentionsEnabled,
    bool FollowsEnabled,
    bool ModerationEnabled,
    bool BotEnabled,
    bool SystemEnabled
);
