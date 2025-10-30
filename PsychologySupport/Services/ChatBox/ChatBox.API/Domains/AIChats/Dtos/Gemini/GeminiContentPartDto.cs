using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiContentPartDto([property: JsonProperty("text")] string Text);