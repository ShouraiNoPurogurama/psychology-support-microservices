using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;


public record GeminiContentDto(
    [property: JsonProperty("role")]
    string Role,
    [property: JsonProperty("parts")]
    List<GeminiContentPartDto> Parts
);
