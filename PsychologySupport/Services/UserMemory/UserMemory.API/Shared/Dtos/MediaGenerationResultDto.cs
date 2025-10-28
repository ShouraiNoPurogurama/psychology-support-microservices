namespace UserMemory.API.Shared.Dtos;

public record MediaGenerationResultDto(
    string CdnUrl,
    string ProviderJobId // ID của job bên DALL-E, Midjourney... (nếu có)
);