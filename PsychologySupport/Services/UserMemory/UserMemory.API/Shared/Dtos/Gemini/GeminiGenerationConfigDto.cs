namespace UserMemory.API.Shared.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    object ResponseSchema,
    string ResponseMimeType = "application/json"
);
