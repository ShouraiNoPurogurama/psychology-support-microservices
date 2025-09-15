using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.SoftDeletePost;

public record SoftDeletePostCommand(
    Guid PostId,
    Guid DeleterAliasId
) : ICommand<SoftDeletePostResult>;

public record SoftDeletePostResult(
    Guid PostId,
    DateTimeOffset DeletedAt
);
