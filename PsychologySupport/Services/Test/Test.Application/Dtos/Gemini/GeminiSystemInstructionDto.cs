using Newtonsoft.Json;

namespace Test.Application.Dtos.Gemini;

public record GeminiSystemInstructionDto(
    [property: JsonProperty("parts")]
    GeminiContentPartDto Parts
);
