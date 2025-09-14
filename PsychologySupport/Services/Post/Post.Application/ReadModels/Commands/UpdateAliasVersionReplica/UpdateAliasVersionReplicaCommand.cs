using BuildingBlocks.CQRS;

namespace Post.Application.ReadModels.Commands.UpdateAliasVersionReplica;

public record UpdateAliasVersionReplicaCommand(
    Guid AliasId,
    Guid SubjectRef,
    Guid AliasVersionId,
    string Label,
    DateTimeOffset ValidFrom) : ICommand<UpdateAliasVersionReplicaCommandResult>;

public record UpdateAliasVersionReplicaCommandResult(bool IsSuccess);