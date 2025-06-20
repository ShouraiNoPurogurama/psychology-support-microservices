namespace ChatBox.API.Dtos.Gemini;

public record GeminiRequestDto(
    List<GeminiContentDto> Contents,
    GeminiSystemInstructionDto SystemInstruction,
    GeminiGenerationConfigDto GenerationConfig,
    List<GeminiSafetySettingDto> SafetySettings
);