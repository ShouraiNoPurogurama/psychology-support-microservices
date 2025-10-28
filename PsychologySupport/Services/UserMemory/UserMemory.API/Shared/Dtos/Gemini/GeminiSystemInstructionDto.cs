using Newtonsoft.Json;

namespace UserMemory.API.Shared.Dtos.Gemini;

public record GeminiSystemInstructionDto(
    [JsonProperty("parts")]
    GeminiContentPartDto Parts
);
