using Newtonsoft.Json;

namespace Media.Application.Features.Media.Dtos.Gemini;

public record GeminiContentPartDto([JsonProperty("text")] string Text);