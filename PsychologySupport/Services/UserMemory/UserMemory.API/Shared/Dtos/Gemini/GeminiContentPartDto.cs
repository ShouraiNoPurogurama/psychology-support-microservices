using Newtonsoft.Json;

namespace UserMemory.API.Shared.Dtos.Gemini;

public record GeminiContentPartDto([JsonProperty("text")] string Text);