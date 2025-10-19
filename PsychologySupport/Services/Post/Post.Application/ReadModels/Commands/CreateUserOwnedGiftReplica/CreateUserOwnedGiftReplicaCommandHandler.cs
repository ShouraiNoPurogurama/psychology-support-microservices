using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Post.Application.Data;
using Post.Application.ReadModels.Models;

namespace Post.Application.ReadModels.Commands.CreateUserOwnedGiftReplica;

public class CreateUserOwnedGiftReplicaCommandHandler(
    IQueryDbContext dbContext,
    ILogger<CreateUserOwnedGiftReplicaCommandHandler> logger)
    : ICommandHandler<CreateUserOwnedGiftReplicaCommand, CreateUserOwnedGiftReplicaResult>
{
    public async Task<CreateUserOwnedGiftReplicaResult> Handle(
        CreateUserOwnedGiftReplicaCommand request,
        CancellationToken cancellationToken)
    {
        if (request.AliasId == Guid.Empty)
        {
            logger.LogError("Invalid AliasId provided.");
            return new CreateUserOwnedGiftReplicaResult(false);
        }

        if (request.ValidFrom >= request.ValidTo)
        {
            logger.LogError("ValidFrom must be earlier than ValidTo.");
            return new CreateUserOwnedGiftReplicaResult(false);
        }


        var activeGifts = await dbContext.GiftReplicas
            .Where(g => g.IsActive)
            .ToListAsync(cancellationToken);

        if (activeGifts.Count == 0)
        {
            logger.LogWarning("No active gifts found. Skipping creation.");
            return new CreateUserOwnedGiftReplicaResult(true);
        }

        foreach (var gift in activeGifts)
        {
            var existing = await dbContext.UserOwnedGiftReplicas
                .FindAsync([request.AliasId, gift.Id], cancellationToken: cancellationToken);

            if (existing is not null)
            {

                existing.ValidFrom = request.ValidFrom;
                existing.ValidTo = request.ValidTo;
                existing.LastSyncedAt = DateTimeOffset.UtcNow;
            }
            else
            {

                var replica = new UserOwnedGiftReplica
                {
                    AliasId = request.AliasId,
                    GiftId = gift.Id,
                    ValidFrom = request.ValidFrom,
                    ValidTo = request.ValidTo,
                    LastSyncedAt = DateTimeOffset.UtcNow
                };
                dbContext.UserOwnedGiftReplicas.Add(replica);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created or updated {Count} UserOwnedGiftReplica entries for AliasId: {AliasId}",
            activeGifts.Count, request.AliasId);

        return new CreateUserOwnedGiftReplicaResult(true);
    }
}
