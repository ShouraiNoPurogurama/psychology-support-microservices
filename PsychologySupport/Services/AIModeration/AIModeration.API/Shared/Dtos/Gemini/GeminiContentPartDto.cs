using Newtonsoft.Json;

namespace AIModeration.API.Shared.Dtos.Gemini;

public record GeminiContentPartDto([JsonProperty("text")] string Text);