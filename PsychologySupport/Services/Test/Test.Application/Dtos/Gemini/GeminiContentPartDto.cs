using Newtonsoft.Json;

namespace Test.Application.Dtos.Gemini;

public record GeminiContentPartDto([JsonProperty("text")] string Text);