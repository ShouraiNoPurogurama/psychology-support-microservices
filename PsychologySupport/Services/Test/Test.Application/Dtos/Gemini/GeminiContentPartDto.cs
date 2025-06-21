using Newtonsoft.Json;

namespace ChatBox.API.Dtos.Gemini;

public record GeminiContentPartDto([JsonProperty("text")] string Text);