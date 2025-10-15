namespace Alias.API.Aliases.Models.Aliases.Enums;

public static class PreferenceExtensions
{
    /// <summary>
    /// Converts PreferenceTheme enum to lowercase string representation
    /// </summary>
    public static string ToLowerString(this PreferenceTheme theme)
    {
        return theme.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Converts PreferenceLanguage enum to lowercase ISO 639-1 code
    /// </summary>
    public static string ToLanguageCode(this PreferenceLanguage language)
    {
        return language.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Parses a string to PreferenceTheme enum (case-insensitive)
    /// </summary>
    public static bool TryParseTheme(string value, out PreferenceTheme theme)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            theme = PreferenceTheme.Light;
            return false;
        }

        return Enum.TryParse(value, ignoreCase: true, out theme);
    }

    /// <summary>
    /// Parses a language code string to PreferenceLanguage enum (case-insensitive)
    /// </summary>
    public static bool TryParseLanguage(string code, out PreferenceLanguage language)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            language = PreferenceLanguage.EN;
            return false;
        }

        return Enum.TryParse(code, ignoreCase: true, out language);
    }

    /// <summary>
    /// Gets the display name for a language
    /// </summary>
    public static string GetDisplayName(this PreferenceLanguage language)
    {
        return language switch
        {
            PreferenceLanguage.EN => "English",
            PreferenceLanguage.VI => "Tiếng Việt",
            PreferenceLanguage.ES => "Español",
            PreferenceLanguage.FR => "Français",
            PreferenceLanguage.DE => "Deutsch",
            PreferenceLanguage.JA => "日本語",
            PreferenceLanguage.KO => "한국어",
            PreferenceLanguage.ZH => "简体中文",
            PreferenceLanguage.PT => "Português",
            PreferenceLanguage.RU => "Русский",
            PreferenceLanguage.IT => "Italiano",
            PreferenceLanguage.AR => "العربية",
            PreferenceLanguage.HI => "हिन्दी",
            PreferenceLanguage.TH => "ไทย",
            PreferenceLanguage.ID => "Bahasa Indonesia",
            _ => language.ToString()
        };
    }
}
