using BuildingBlocks.CQRS;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Commands.CreatePost;

public record CreatePostCommand(
    Guid IdempotencyKey,
    string? Title,
    string Content,
    PostVisibility Visibility,
    IEnumerable<Guid>? MediaIds = null,
    Guid? CategoryTagId = null,
    Guid? EmotionId = null
) : IdempotentCommand<CreatePostResult>(IdempotencyKey);

public record CreatePostResult(
    Guid Id,
    string ModerationStatus,
    DateTimeOffset CreatedAt
);
