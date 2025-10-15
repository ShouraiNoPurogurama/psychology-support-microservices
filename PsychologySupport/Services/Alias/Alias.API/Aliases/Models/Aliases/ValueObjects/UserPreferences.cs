using Alias.API.Aliases.Models.Aliases.Enums;

namespace Alias.API.Aliases.Models.Aliases.ValueObjects;

public sealed record UserPreferences
{
    public PreferenceTheme Theme { get; private init; } = PreferenceTheme.Light;
    public PreferenceLanguage Language { get; private init; } = PreferenceLanguage.EN;
    public bool NotificationsEnabled { get; private init; } = true;

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

    public UserPreferences UpdateTheme(PreferenceTheme newTheme)
    {
        return this with { Theme = newTheme };
    }

    public UserPreferences UpdateLanguage(PreferenceLanguage newLanguage)
    {
        return this with { Language = newLanguage };
    }

    public UserPreferences UpdateNotificationsEnabled(bool enabled)
    {
        return this with { NotificationsEnabled = enabled };
    }

    public UserPreferences Update(PreferenceTheme? theme = null, PreferenceLanguage? language = null, bool? notificationsEnabled = null)
    {
        var updatedPreferences = this;

        if (theme.HasValue)
        {
            updatedPreferences = updatedPreferences.UpdateTheme(theme.Value);
        }

        if (language.HasValue)
        {
            updatedPreferences = updatedPreferences.UpdateLanguage(language.Value);
        }

        if (notificationsEnabled.HasValue)
        {
            updatedPreferences = updatedPreferences.UpdateNotificationsEnabled(notificationsEnabled.Value);
        }

        return updatedPreferences;
    }
}
