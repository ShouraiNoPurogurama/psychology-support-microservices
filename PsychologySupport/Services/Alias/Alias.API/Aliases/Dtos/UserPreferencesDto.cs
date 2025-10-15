using Alias.API.Aliases.Models.Aliases.Enums;

namespace Alias.API.Aliases.Dtos;

public record UserPreferencesDto(
    PreferenceTheme Theme,
    PreferenceLanguage Language,
    bool NotificationsEnabled);
