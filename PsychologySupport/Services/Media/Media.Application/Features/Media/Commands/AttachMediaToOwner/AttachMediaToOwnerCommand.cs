using BuildingBlocks.CQRS;

namespace Media.Application.Features.Media.Commands.AttachMediaToOwner;

public record AttachMediaToOwnerCommand(Guid OwnerId, string OwnerType, Guid MediaId)
    : ICommand<AttachMediaToOwnerResult>;

public record AttachMediaToOwnerResult(bool IsSuccess);