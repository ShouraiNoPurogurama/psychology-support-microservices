using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Post.Application.Data;
using Post.Application.ReadModels.Models;

namespace Post.Application.ReadModels.Commands.CreateUserOwnedTagReplica
{
    public class CreateUserOwnedTagReplicaCommandHandler(
        IQueryDbContext dbContext,
        ILogger<CreateUserOwnedTagReplicaCommandHandler> logger)
        : ICommandHandler<CreateUserOwnedTagReplicaCommand, CreateUserOwnedTagReplicaResult>
    {
        public async Task<CreateUserOwnedTagReplicaResult> Handle(
            CreateUserOwnedTagReplicaCommand request,
            CancellationToken cancellationToken)
        {
            if (request.AliasId == Guid.Empty)
            {
                logger.LogError("Invalid AliasId provided.");
                return new CreateUserOwnedTagReplicaResult(false);
            }

            if (request.ValidFrom >= request.ValidTo)
            {
                logger.LogError("ValidFrom must be earlier than ValidTo.");
                return new CreateUserOwnedTagReplicaResult(false);
            }


            var activeTags = await dbContext.EmotionTagReplicas
                .Where(t => t.IsActive)
                .ToListAsync(cancellationToken);

            if (activeTags.Count == 0)
            {
                logger.LogWarning("No active emotion tags found. Skipping creation.");
                return new CreateUserOwnedTagReplicaResult(true);
            }

            foreach (var tag in activeTags)
            {
                var existing = await dbContext.UserOwnedTagReplicas
                    .FindAsync([request.AliasId, tag.Id], cancellationToken: cancellationToken);

                if (existing is not null)
                {
     
                    existing.ValidFrom = request.ValidFrom;
                    existing.ValidTo = request.ValidTo;
                    existing.LastSyncedAt = DateTimeOffset.UtcNow;
                }
                else
                {

                    var replica = new UserOwnedTagReplica
                    {
                        AliasId = request.AliasId,
                        EmotionTagId = tag.Id,
                        ValidFrom = request.ValidFrom,
                        ValidTo = request.ValidTo,
                        LastSyncedAt = DateTimeOffset.UtcNow
                    };
                    dbContext.UserOwnedTagReplicas.Add(replica);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Created or updated {Count} UserOwnedTagReplica entries for AliasId: {AliasId}",
                activeTags.Count, request.AliasId);

            return new CreateUserOwnedTagReplicaResult(true);
        }
    }
}
