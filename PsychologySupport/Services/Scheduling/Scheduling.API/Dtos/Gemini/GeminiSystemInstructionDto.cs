using Newtonsoft.Json;

namespace Scheduling.API.Dtos.Gemini;

public record GeminiSystemInstructionDto(
    [JsonProperty("parts")]
    GeminiContentPartDto Parts
);
