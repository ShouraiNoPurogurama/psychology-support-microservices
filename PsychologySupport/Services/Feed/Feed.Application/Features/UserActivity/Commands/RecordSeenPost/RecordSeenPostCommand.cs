using BuildingBlocks.CQRS;

namespace Feed.Application.Features.UserActivity.Commands.RecordSeenPost;

public record RecordSeenPostCommand(
    Guid IdempotencyKey,
    Guid AliasId,
    Guid PostId,
    DateOnly Date,
    int DwellTimeMs
) : IdempotentCommand<RecordSeenPostResult>(IdempotencyKey);

public record RecordSeenPostResult(
    bool Success,
    string Message
);
