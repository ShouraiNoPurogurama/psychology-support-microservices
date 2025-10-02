using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Commands.AbandonPost;

/// <summary>
/// Command to mark posts that were started but never completed as abandoned.
/// This triggers the Emo Bot in the Moderation service.
/// </summary>
public record AbandonPostCommand(
    Guid PostId
) : ICommand<AbandonPostResult>;

public record AbandonPostResult(
    Guid PostId,
    string Status,
    DateTimeOffset AbandonedAt
);
