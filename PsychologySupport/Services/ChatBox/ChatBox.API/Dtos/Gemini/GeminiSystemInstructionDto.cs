namespace ChatBox.API.Dtos.Gemini;

public record GeminiSystemInstructionDto(
    List<GeminiContentPartDto> Parts
);
