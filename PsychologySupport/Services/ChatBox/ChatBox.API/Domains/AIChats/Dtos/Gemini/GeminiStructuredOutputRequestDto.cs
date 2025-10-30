using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiStructuredOutputRequestDto(
    [property: JsonProperty("contents")]
    List<GeminiContentDto> Contents,
    [property: JsonProperty("system_instruction")]
    GeminiSystemInstructionDto SystemInstruction,
    [property: JsonProperty("generationConfig")]
    GeminiStructuredOutputGenerationConfigDto GenerationConfig,
    [property: JsonProperty("safetySettings")]
    List<GeminiSafetySettingDto> SafetySettings
    );