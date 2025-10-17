using AIModeration.API.Models;
using AIModeration.API.Shared.Dtos;

namespace AIModeration.API.Shared.Services;

public interface IContentModerationService
{
    Task<PostModerationResultDto> ModeratePostContentAsync(Guid postId, string? title, string content, CancellationToken cancellationToken = default);
    Task<AliasLabelModerationResultDto> ModerateAliasLabelAsync(string label, CancellationToken cancellationToken = default);
}
