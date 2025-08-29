using Newtonsoft.Json;

namespace Translation.API.Domains.Translations.Dtos.Gemini;

public record GeminiContentPartDto([JsonProperty("text")] string Text);