using BuildingBlocks.CQRS;

namespace Post.Application.ReadModels.Commands;

public record CreateAliasVersionReplicaCommand(
    Guid AliasId,
    Guid SubjectRef,
    Guid AliasVersionId,
    string Label,
    DateTimeOffset ValidFrom) : ICommand<CreateAliasVersionReplicaResult>;

public record CreateAliasVersionReplicaResult(bool IsSuccess);