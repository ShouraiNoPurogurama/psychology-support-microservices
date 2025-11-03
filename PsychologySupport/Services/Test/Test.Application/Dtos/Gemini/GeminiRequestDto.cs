using Newtonsoft.Json;

namespace Test.Application.Dtos.Gemini;

public record GeminiRequestDto(
    [property: JsonProperty("contents")]
    List<GeminiContentDto> Contents,
    [property: JsonProperty("system_instruction")]
    GeminiSystemInstructionDto SystemInstruction,
    [property: JsonProperty("generationConfig")]
    GeminiGenerationConfigDto GenerationConfig
);