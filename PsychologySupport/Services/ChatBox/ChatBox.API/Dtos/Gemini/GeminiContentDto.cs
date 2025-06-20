namespace ChatBox.API.Dtos.Gemini;

public record GeminiContentDto(
    string Role,
    List<GeminiContentPartDto> Parts
);
