using Newtonsoft.Json;

namespace Media.Application.Features.Media.Dtos.Gemini;

public record GeminiSystemInstructionDto(
    [JsonProperty("parts")]
    GeminiContentPartDto Parts
);
