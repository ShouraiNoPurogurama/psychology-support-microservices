using BuildingBlocks.CQRS;
using Mapster;
using Microsoft.Extensions.Logging;
using Post.Application.Data;
using Post.Application.ReadModels.Models;

namespace Post.Application.ReadModels.Commands.UpdateAliasVersionReplica;

public class UpdateAliasVersionReplicaCommandHandler(
    IQueryDbContext dbContext,
    ILogger<UpdateAliasVersionReplicaCommandHandler> logger)
    : ICommandHandler<UpdateAliasVersionReplicaCommand, UpdateAliasVersionReplicaCommandResult>
{
    public async Task<UpdateAliasVersionReplicaCommandResult> Handle(UpdateAliasVersionReplicaCommand request,
        CancellationToken cancellationToken)
    {
        if (request.AliasId == Guid.Empty ||
            request.SubjectRef == Guid.Empty ||
            request.AliasVersionId == Guid.Empty)
        {
            logger.LogError("Invalid Id(s) provided.");
            return new UpdateAliasVersionReplicaCommandResult(false);
        }

        if (string.IsNullOrWhiteSpace(request.Label) ||
            request.Label.Length > 30)

        {
            logger.LogError("Invalid alias label provided.");
            return new UpdateAliasVersionReplicaCommandResult(false);
        }

        if (
            request.ValidFrom > DateTimeOffset.UtcNow)
        {
            logger.LogError("Invalid ValidFrom provided.");
            return new UpdateAliasVersionReplicaCommandResult(false);
        }

        var existing = await dbContext.AliasVersionReplica
            .FindAsync([request.AliasId], cancellationToken: cancellationToken);

        if (existing is null)
        {
            logger.LogInformation("Existing Alias Version Replica not found. Skipping update.");
            return new UpdateAliasVersionReplicaCommandResult(false);
        }

        request.Adapt(existing);
        existing.LastSyncedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateAliasVersionReplicaCommandResult(true);
    }
}