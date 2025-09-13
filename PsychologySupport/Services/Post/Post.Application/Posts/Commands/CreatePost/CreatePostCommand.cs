using BuildingBlocks.CQRS;
using Post.Domain.Enums;

namespace Post.Application.Posts.Commands.CreatePost;

public sealed record CreatePostCommand(
    Guid RequestKey,                          //từ header "Idempotency-Key"
    string? Title,
    string Content,
    string Visibility,
    IEnumerable<Guid>? MediaIds
) : IdempotentCommand<CreatePostResult>(RequestKey);

public sealed record CreatePostResult(
    Guid Id,
    string ModerationStatus,
    DateTimeOffset CreatedAt);