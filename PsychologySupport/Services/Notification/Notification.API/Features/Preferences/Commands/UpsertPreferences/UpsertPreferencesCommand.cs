using BuildingBlocks.CQRS;

namespace Notification.API.Features.Preferences.Commands.UpsertPreferences;

public record UpsertPreferencesCommand(
    Guid AliasId,
    bool ReactionsEnabled,
    bool CommentsEnabled,
    bool MentionsEnabled,
    bool FollowsEnabled,
    bool ModerationEnabled,
    bool BotEnabled,
    bool SystemEnabled
) : ICommand<UpsertPreferencesResult>;

public record UpsertPreferencesResult(bool Success);
