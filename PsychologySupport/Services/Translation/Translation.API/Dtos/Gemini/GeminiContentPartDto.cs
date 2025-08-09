using Newtonsoft.Json;

namespace Translation.API.Dtos.Gemini;

public record GeminiContentPartDto([JsonProperty("text")] string Text);