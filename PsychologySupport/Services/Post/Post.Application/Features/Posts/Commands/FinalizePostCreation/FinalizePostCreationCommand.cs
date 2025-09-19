using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Commands.FinalizePostCreation;


public record FinalizePostCreationCommand(Guid PostId)
    : ICommand<FinalizePostCreationResult>;

public record FinalizePostCreationResult(bool IsSuccess);