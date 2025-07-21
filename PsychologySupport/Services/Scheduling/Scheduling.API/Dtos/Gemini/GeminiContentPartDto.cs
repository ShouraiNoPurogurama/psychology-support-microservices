using Newtonsoft.Json;

namespace Scheduling.API.Dtos.Gemini;

public record GeminiContentPartDto([JsonProperty("text")] string Text);