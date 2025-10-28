namespace UserMemory.API.Shared.Dtos;

public record RewardGenerationDataDto(
    Guid UserId,
    string FinalPrompt,
    string PromptBase, // Prompt gốc (để lưu lại)
    string PromptFiller // Phần "cảm xúc/sự kiện" (để lưu lại);
);