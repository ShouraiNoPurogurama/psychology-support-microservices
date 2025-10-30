using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiSystemInstructionDto(
    [property: JsonProperty("parts")]
    GeminiContentPartDto Parts
);
