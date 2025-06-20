namespace ChatBox.API.Dtos.Gemini;

public record GeminiSafetySettingDto(
    string Category,
    string Threshold = "OFF"
);
