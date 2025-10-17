using System.Text.Json.Serialization;

namespace Alias.API.Aliases.Models.Aliases.Enums;

/// <summary>
/// Supported languages for user interface (ISO 639-1 codes)
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PreferenceLanguage
{
    /// <summary>English</summary>
    EN = 1,
    
    /// <summary>Vietnamese (Tiếng Việt)</summary>
    VI = 2,
    
    /// <summary>Spanish (Español)</summary>
    ES = 3,
    
    /// <summary>French (Français)</summary>
    FR = 4,
    
    /// <summary>German (Deutsch)</summary>
    DE = 5,
    
    /// <summary>Japanese (日本語)</summary>
    JA = 6,
    
    /// <summary>Korean (한국어)</summary>
    KO = 7,
    
    /// <summary>Chinese Simplified (简体中文)</summary>
    ZH = 8,
    
    /// <summary>Portuguese (Português)</summary>
    PT = 9,
    
    /// <summary>Russian (Русский)</summary>
    RU = 10,
    
    /// <summary>Italian (Italiano)</summary>
    IT = 11,
    
    /// <summary>Arabic (العربية)</summary>
    AR = 12,
    
    /// <summary>Hindi (हिन्दी)</summary>
    HI = 13,
    
    /// <summary>Thai (ไทย)</summary>
    TH = 14,
    
    /// <summary>Indonesian (Bahasa Indonesia)</summary>
    ID = 15
}
