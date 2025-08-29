using Newtonsoft.Json;

namespace Translation.API.Domains.Translations.Dtos.Gemini;


public record GeminiContentDto(
    [JsonProperty("role")]
    string Role,
    [JsonProperty("parts")]
    List<GeminiContentPartDto> Parts
);
