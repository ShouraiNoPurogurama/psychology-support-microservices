using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.CreatePost;

public record CreatePostCommand(
    Guid IdempotencyKey,
    string? Title,
    string Content,
    string Visibility,
    IEnumerable<Guid>? MediaIds
) : IdempotentCommand<CreatePostResult>(IdempotencyKey);

public record CreatePostResult(
    Guid Id,
    string ModerationStatus,
    DateTimeOffset CreatedAt
);
