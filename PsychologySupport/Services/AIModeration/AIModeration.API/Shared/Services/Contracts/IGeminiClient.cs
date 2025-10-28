using AIModeration.API.Shared.Dtos;

namespace AIModeration.API.Shared.Services.Contracts;

public interface IGeminiClient
{
    Task<ModerationResultDto> ModeratePostContentAsync(string postContent);
    Task<ModerationResultDto> ModerateAliasLabelAsync(string aliasLabel);
}