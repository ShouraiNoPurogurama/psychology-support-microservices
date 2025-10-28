using Newtonsoft.Json;

namespace Media.Application.Features.Media.Dtos.Gemini;


public record GeminiContentDto(
    [JsonProperty("role")]
    string Role,
    [JsonProperty("parts")]
    List<GeminiContentPartDto> Parts
);
