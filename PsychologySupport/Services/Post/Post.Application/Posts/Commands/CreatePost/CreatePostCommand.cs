using BuildingBlocks.CQRS;
using Post.Domain.Enums;

namespace Post.Application.Posts.Commands.CreatePost;

public record CreatePostCommand(
    string Content,
    PostVisibility Visibility,
    IEnumerable<Guid>? MediaIds) : ICommand<CreatePostResult>;

public record CreatePostResult(
    Guid Id,
    string ModerationStatus,
    DateTimeOffset CreatedAt);