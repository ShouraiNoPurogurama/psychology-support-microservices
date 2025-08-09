namespace Scheduling.API.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    object ResponseSchema,
    string ResponseMimeType = "application/json"
);
