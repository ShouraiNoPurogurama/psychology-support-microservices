using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiContentPartDto([JsonProperty("text")] string Text);