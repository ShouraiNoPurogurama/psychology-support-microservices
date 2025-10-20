using BuildingBlocks.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Post.Application.ReadModels.Commands.CreateUserOwnedGiftReplica
{
    public record CreateUserOwnedGiftReplicaCommand(
        Guid AliasId,
        DateTimeOffset ValidFrom,
        DateTimeOffset ValidTo
    ) : ICommand<CreateUserOwnedGiftReplicaResult>;

    public record CreateUserOwnedGiftReplicaResult(bool IsSuccess);
}
