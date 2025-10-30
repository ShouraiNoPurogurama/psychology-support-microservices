using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiSafetySettingDto(
    [property: JsonProperty("category")]
    string Category,
    [property: JsonProperty("threshold")]
    string Threshold = "OFF"
);
