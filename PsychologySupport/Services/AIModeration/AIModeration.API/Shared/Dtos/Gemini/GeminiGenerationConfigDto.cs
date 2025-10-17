namespace AIModeration.API.Shared.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    object ResponseSchema,
    string ResponseMimeType = "application/json"
);
