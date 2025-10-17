using Alias.API.Aliases.Models.Aliases.Enums;

namespace Alias.API.Aliases.Models.Aliases.ValueObjects;

public sealed class UserPreferences
{
    public PreferenceTheme Theme { get; private set; } = PreferenceTheme.Light;
    public PreferenceLanguage Language { get; private set; } = PreferenceLanguage.EN;
    public bool NotificationsEnabled { get; private set; } = true;

    private UserPreferences()
    {
    }

    private UserPreferences(PreferenceTheme theme, PreferenceLanguage language, bool notificationsEnabled)
    {
        Theme = theme;
        Language = language;
        NotificationsEnabled = notificationsEnabled;
    }

    public static UserPreferences CreateDefault()
    {
        return new UserPreferences(
            theme: PreferenceTheme.Light,
            language: PreferenceLanguage.EN,
            notificationsEnabled: true);
    }

    public static UserPreferences Create(PreferenceTheme theme, PreferenceLanguage language, bool notificationsEnabled)
    {
        return new UserPreferences(
            theme: theme,
            language: language,
            notificationsEnabled: notificationsEnabled);
    }

    public void Update(
        PreferenceTheme? theme = null,
        PreferenceLanguage? language = null,
        bool? notificationsEnabled = null)
    {
        if (theme.HasValue) Theme = theme.Value;
        if (language.HasValue) Language = language.Value;
        if (notificationsEnabled.HasValue) NotificationsEnabled = notificationsEnabled.Value;
    }
    
    public override bool Equals(object? obj) =>
        obj is UserPreferences other &&
        Theme == other.Theme &&
        Language == other.Language &&
        NotificationsEnabled == other.NotificationsEnabled;

    public override int GetHashCode() =>
        HashCode.Combine(Theme, Language, NotificationsEnabled);
}
