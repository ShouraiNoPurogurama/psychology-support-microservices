﻿using Newtonsoft.Json;

namespace ChatBox.API.Dtos.Gemini;

public record GeminiRequestDto(
    [JsonProperty("contents")]
    List<GeminiContentDto> Contents,
    [JsonProperty("systemInstruction")]
    GeminiSystemInstructionDto SystemInstruction,
    [JsonProperty("generationConfig")]
    GeminiGenerationConfigDto GenerationConfig,
    [JsonProperty("safetySettings")]
    List<GeminiSafetySettingDto> SafetySettings
);