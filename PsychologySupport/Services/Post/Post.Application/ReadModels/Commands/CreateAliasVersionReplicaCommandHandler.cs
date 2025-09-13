using BuildingBlocks.CQRS;
using Microsoft.Extensions.Logging;
using Post.Application.Data;
using Post.Application.ReadModels.Models;

namespace Post.Application.ReadModels.Commands;

public class CreateAliasVersionReplicaCommandHandler(
    IQueryDbContext dbContext,
    ILogger<CreateAliasVersionReplicaCommandHandler> logger)
    : ICommandHandler<CreateAliasVersionReplicaCommand, CreateAliasVersionReplicaResult>
{
    public async Task<CreateAliasVersionReplicaResult> Handle(CreateAliasVersionReplicaCommand request,
        CancellationToken cancellationToken)
    {
        if (request.AliasId == Guid.Empty ||
            request.SubjectRef == Guid.Empty ||
            request.AliasVersionId == Guid.Empty)
        {
            logger.LogError("Invalid Id(s) provided.");
            return new CreateAliasVersionReplicaResult(false);
        }

        if (string.IsNullOrWhiteSpace(request.Label) ||
            request.Label.Length > 30)

        {
            logger.LogError("Invalid alias label provided.");
            return new CreateAliasVersionReplicaResult(false);
        }

        if (
            request.ValidFrom > DateTimeOffset.UtcNow)
        {
            logger.LogError("Invalid ValidFrom provided.");
            return new CreateAliasVersionReplicaResult(false);
        }
        
        
        var existing = await dbContext.AliasVersionReplica
            .FindAsync([request.AliasId], cancellationToken: cancellationToken);
        
        if(existing is not null) 
        {
            logger.LogInformation("AliasVersionReplica already exists. Skipping creation.");
            return new CreateAliasVersionReplicaResult(true);
        }
        
        var replica = new AliasVersionReplica
        {
            AliasId = request.AliasId,
            CurrentVersionId = request.AliasVersionId,
            Label = request.Label,
            ValidFrom = request.ValidFrom,
            LastSyncedAt = DateTimeOffset.UtcNow
        };
        
        dbContext.AliasVersionReplica.Add(replica);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return new CreateAliasVersionReplicaResult(true);
    }
}