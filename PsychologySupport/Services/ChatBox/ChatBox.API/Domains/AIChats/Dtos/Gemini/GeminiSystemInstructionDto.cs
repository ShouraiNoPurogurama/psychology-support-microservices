using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiSystemInstructionDto(
    [JsonProperty("parts")]
    GeminiContentPartDto Parts
);
