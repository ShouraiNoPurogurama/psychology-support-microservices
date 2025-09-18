using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Commands.EditPost;

public record EditPostCommand(
    Guid AliasId,
    Guid PostId,
    string Content,
    string? Title,
    List<string>? MediaUrls,
    List<Guid>? CategoryTagIds
) : ICommand<EditPostResult>;

public record EditPostResult(
    Guid PostId,
    DateTimeOffset UpdatedAt
);
