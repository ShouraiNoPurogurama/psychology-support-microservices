using Newtonsoft.Json;

namespace AIModeration.API.Shared.Dtos.Gemini;

public record GeminiSystemInstructionDto(
    [JsonProperty("parts")]
    GeminiContentPartDto Parts
);
