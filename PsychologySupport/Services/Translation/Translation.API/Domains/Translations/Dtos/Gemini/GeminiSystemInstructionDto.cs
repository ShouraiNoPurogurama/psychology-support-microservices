using Newtonsoft.Json;

namespace Translation.API.Domains.Translations.Dtos.Gemini;

public record GeminiSystemInstructionDto(
    [JsonProperty("parts")]
    GeminiContentPartDto Parts
);
