namespace ChatBox.API.Domains.AIChats.Dtos.Gemini;

public record GeminiStructuredOutputGenerationConfigDto(
    object ResponseSchema,
    string ResponseMimeType = "application/json"
);
