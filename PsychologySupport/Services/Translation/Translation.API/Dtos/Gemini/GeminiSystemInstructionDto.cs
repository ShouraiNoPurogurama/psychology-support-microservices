using Newtonsoft.Json;

namespace Translation.API.Dtos.Gemini;

public record GeminiSystemInstructionDto(
    [JsonProperty("parts")]
    GeminiContentPartDto Parts
);
