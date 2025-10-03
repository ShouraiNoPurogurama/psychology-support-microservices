using BuildingBlocks.CQRS;
using Post.Application.Features.Posts.Commands.CreatePost;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Posts.Commands.AttachMediaToPost;

public record AttachMediaToPostCommand(
    Guid IdempotencyKey,
    Guid PostId,
    MediaItemDto Media,
    int? Position = null
) : IdempotentCommand<AttachMediaToPostResult>(IdempotencyKey);

public record AttachMediaToPostResult(
    Guid PostId,
    MediaItemDto Media,
    DateTimeOffset AttachedAt
);
