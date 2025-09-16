using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.UpdateMediaAltText;

public record UpdateMediaAltTextCommand(
    Guid IdempotencyKey,
    Guid MediaId,
    string AltText
) : IdempotentCommand<UpdateMediaAltTextResult>(IdempotencyKey);

public record UpdateMediaAltTextResult(
    Guid MediaId,
    string AltText,
    DateTimeOffset UpdatedAt
);
