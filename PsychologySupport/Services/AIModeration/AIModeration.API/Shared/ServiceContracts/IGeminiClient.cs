using AIModeration.API.Shared.Dtos;

namespace AIModeration.API.Shared.ServiceContracts;

public interface IGeminiClient
{
    Task<PostContentModerationResultDto> ModeratePostContentAsync(string postContent);
}