using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiSafetySettingDto(
    [JsonProperty("category")]
    string Category,
    [JsonProperty("threshold")]
    string Threshold = "OFF"
);
