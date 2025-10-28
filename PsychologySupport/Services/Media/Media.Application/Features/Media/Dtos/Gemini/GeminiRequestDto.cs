using Newtonsoft.Json;

namespace Media.Application.Features.Media.Dtos.Gemini;

public record GeminiRequestDto(
    [JsonProperty("contents")]
    List<GeminiContentDto> Contents,
    [JsonProperty("systemInstruction")]
    GeminiSystemInstructionDto SystemInstruction,
    [JsonProperty("generationConfig")]
    GeminiGenerationConfigDto GenerationConfig
);