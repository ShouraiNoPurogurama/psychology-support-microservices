using BuildingBlocks.CQRS;

namespace Media.Application.Features.Media.Commands.BatchAttachMediaToOwner;

public record BatchAttachMediaToOwnerCommand(Guid OwnerId, string OwnerType, IEnumerable<Guid> MediaIds)
    : ICommand<BatchAttachMediaToOwnerResult>;
    
    
public record BatchAttachMediaToOwnerResult(bool IsSuccess);