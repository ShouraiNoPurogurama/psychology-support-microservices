using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Commands.DeletePost;

public record DeletePostCommand(Guid PostId) : ICommand<DeletePostResult>;

public record DeletePostResult(
    Guid PostId,
    DateTimeOffset DeletedAt
);
