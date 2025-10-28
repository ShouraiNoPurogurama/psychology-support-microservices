using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiStructuredOutputRequestDto(
    [JsonProperty("contents")]
    List<GeminiContentDto> Contents,
    [JsonProperty("systemInstruction")]
    GeminiSystemInstructionDto SystemInstruction,
    [JsonProperty("generationConfig")]
    GeminiStructuredOutputGenerationConfigDto GenerationConfig,
    [JsonProperty("safetySettings")]
    List<GeminiSafetySettingDto> SafetySettings
    );