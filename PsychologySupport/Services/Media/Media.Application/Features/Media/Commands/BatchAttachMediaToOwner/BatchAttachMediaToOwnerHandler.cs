using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Media;
using MassTransit;
using Media.Application.Data;
using Media.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Media.Application.Features.Media.Commands.BatchAttachMediaToOwner;

public class BatchAttachMediaToOwnerHandler(
    IMediaDbContext dbContext,
    ILogger<BatchAttachMediaToOwnerHandler> logger,
    IPublishEndpoint publishEndpoint) // Inject IPublishEndpoint
    : ICommandHandler<BatchAttachMediaToOwnerCommand, BatchAttachMediaToOwnerResult>
{
    public async Task<BatchAttachMediaToOwnerResult> Handle(BatchAttachMediaToOwnerCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MediaOwnerType>(request.OwnerType, true, out var ownerType))
        {
            logger.LogWarning("Invalid owner type received: {OwnerType}", request.OwnerType);

            await publishEndpoint.Publish(new MediaAttachmentFailedIntegrationEvent(
                request.OwnerId,
                request.OwnerType,
                request.MediaIds.ToDictionary(id => id, id => "Invalid OwnerType"),
                "Invalid OwnerType provided."
            ), cancellationToken);

            return new BatchAttachMediaToOwnerResult(false);
        }

        var mediaAssets = await dbContext.MediaAssets
            .Include(m => m.Owners)
            .Where(m => request.MediaIds.Contains(m.Id))
            .ToListAsync(cancellationToken);

        var foundMediaIds = mediaAssets.Select(m => m.Id).ToHashSet();
        var notFoundMediaIds = request.MediaIds.Where(id => !foundMediaIds.Contains(id)).ToList();

        var succeededMediaIds = new List<Guid>();
        var failedMedia = new Dictionary<Guid, string>();

        foreach (var id in notFoundMediaIds)
        {
            failedMedia[id] = "Media not found.";
        }

        foreach (var media in mediaAssets)
        {
            //Thêm Validation trạng thái của Media
            if (media.State is MediaState.Blocked or MediaState.Deleted)
            {
                failedMedia[media.Id] = $"Cannot attach media in '{media.State}' state.";
                continue;
            }

            bool isAlreadyOwned = media.Owners.Any(o => o.MediaOwnerType == ownerType && o.MediaOwnerId == request.OwnerId);
            if (isAlreadyOwned)
            {
                //Đã được gán từ trước, coi như thành công và bỏ qua
                succeededMediaIds.Add(media.Id);
                continue;
            }

            //Gán ownership
            media.AssignOwnership(ownerType, request.OwnerId);
            succeededMediaIds.Add(media.Id);
        }

        if (succeededMediaIds.Count > 0 || foundMediaIds.Count > 0) 
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (succeededMediaIds.Any())
        {
            logger.LogInformation("Successfully attached {Count} media items to owner {OwnerId}", succeededMediaIds.Count,
                request.OwnerId);
            await publishEndpoint.Publish(
                new MediaAttachedToOwnerIntegrationEvent(request.OwnerId, request.OwnerType, succeededMediaIds),
                cancellationToken);
        }

        if (failedMedia.Any())
        {
            logger.LogWarning("Failed to attach {Count} media items to owner {OwnerId}", failedMedia.Count, request.OwnerId);
            await publishEndpoint.Publish(
                new MediaAttachmentFailedIntegrationEvent(request.OwnerId, request.OwnerType, failedMedia,
                    "One or more media items failed to attach."), cancellationToken);
        }

        return new BatchAttachMediaToOwnerResult(!failedMedia.Any());
    }
}