namespace Media.Application.Features.Media.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    object ResponseSchema,
    string ResponseMimeType = "application/json"
);
