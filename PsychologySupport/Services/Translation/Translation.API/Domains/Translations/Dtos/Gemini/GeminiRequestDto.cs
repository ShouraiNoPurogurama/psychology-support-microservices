using Newtonsoft.Json;

namespace Translation.API.Domains.Translations.Dtos.Gemini;

public record GeminiRequestDto(
    [JsonProperty("contents")]
    List<GeminiContentDto> Contents,
    [JsonProperty("systemInstruction")]
    GeminiSystemInstructionDto SystemInstruction,
    [JsonProperty("generationConfig")]
    GeminiGenerationConfigDto GenerationConfig
);