using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.UpdatePost;

public record UpdatePostCommand(
    Guid PostId,
    string NewContent,
    string? NewTitle,
    Guid EditorAliasId
) : ICommand<UpdatePostResult>;

public record UpdatePostResult(
    Guid PostId,
    DateTimeOffset UpdatedAt
);
