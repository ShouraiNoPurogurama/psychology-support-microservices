using Newtonsoft.Json;

namespace Test.Application.Dtos.Gemini;

public record GeminiContentPartDto([property: JsonProperty("text")] string Text);