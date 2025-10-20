using BuildingBlocks.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Post.Application.ReadModels.Commands.CreateUserOwnedTagReplica
{
    public record CreateUserOwnedTagReplicaCommand(
        Guid AliasId,
        DateTimeOffset ValidFrom,
        DateTimeOffset ValidTo
    ) : ICommand<CreateUserOwnedTagReplicaResult>;

    public record CreateUserOwnedTagReplicaResult(bool IsSuccess);
}
