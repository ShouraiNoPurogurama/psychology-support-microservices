namespace ChatBox.API.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    double Temperature = 1.0,
    double TopP = 0.95,
    int MaxOutputTokens = 8192
);
