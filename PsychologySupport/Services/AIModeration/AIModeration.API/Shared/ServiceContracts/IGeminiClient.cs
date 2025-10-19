using AIModeration.API.Shared.Dtos;

namespace AIModeration.API.Shared.ServiceContracts;

public interface IGeminiClient
{
    Task<ModerationResultDto> ModeratePostContentAsync(string postContent);
    Task<ModerationResultDto> ModerateAliasLabelAsync(string aliasLabel);
}