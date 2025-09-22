using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Media;
using MassTransit;
using Media.Application.Data;
using Media.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Media.Application.Features.Media.Commands.AttachMediaToOwner;

public class AttachMediaToOwnerHandler(
    IMediaDbContext dbContext,
    ILogger<AttachMediaToOwnerHandler> logger,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<AttachMediaToOwnerCommand, AttachMediaToOwnerResult>
{
    public async Task<AttachMediaToOwnerResult> Handle(AttachMediaToOwnerCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate OwnerType
        if (!Enum.TryParse<MediaOwnerType>(request.OwnerType, true, out var ownerType))
        {
            logger.LogWarning("Invalid owner type received: {OwnerType}", request.OwnerType);
            await PublishFailureEventAsync(request,"Invalid OwnerType provided.", "Invalid OwnerType provided.", cancellationToken);
            return new AttachMediaToOwnerResult(false);
        }

        // 2. Tìm một media duy nhất
        var mediaAsset = await dbContext.MediaAssets
            .Include(m => m.Owners)
            .FirstOrDefaultAsync(m => m.Id == request.MediaId, cancellationToken);

        // 3. Xử lý trường hợp không tìm thấy
        if (mediaAsset is null)
        {
            logger.LogWarning("Media not found for attachment. MediaId: {MediaId}", request.MediaId);
            await PublishFailureEventAsync(request,"Media not found.", "Media not found.", cancellationToken);
            return new AttachMediaToOwnerResult(false);
        }

        // 4. Validate trạng thái của media
        if (mediaAsset.State is MediaState.Blocked or MediaState.Deleted)
        {
            var reason = $"Cannot attach media in '{mediaAsset.State}' state.";
            logger.LogWarning("Invalid media state for attachment. MediaId: {MediaId}, State: {State}", request.MediaId,
                mediaAsset.State);
            await PublishFailureEventAsync(request, reason, reason, cancellationToken);
            return new AttachMediaToOwnerResult(false);
        }

        // 5. Kiểm tra Idempotency (đã được sở hữu)
        bool isAlreadyOwned = mediaAsset.Owners.Any(o => o.MediaOwnerType == ownerType && o.MediaOwnerId == request.OwnerId);
        if (isAlreadyOwned)
        {
            logger.LogInformation("Media {MediaId} is already attached to owner {OwnerId}. Operation is idempotent.",
                request.MediaId, request.OwnerId);
            await PublishSuccessEventAsync(request, cancellationToken);
            return new AttachMediaToOwnerResult(true);
        }

        // 6. Gán ownership, lưu và publish event thành công
        mediaAsset.AssignOwnership(ownerType, request.OwnerId);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully attached media {MediaId} to owner {OwnerId}", request.MediaId, request.OwnerId);
        await PublishSuccessEventAsync(request, cancellationToken);

        return new AttachMediaToOwnerResult(true);
    }

    private Task PublishSuccessEventAsync(AttachMediaToOwnerCommand request, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(
            new MediaAttachedToOwnerIntegrationEvent(request.OwnerId, request.OwnerType, new List<Guid> { request.MediaId }),
            cancellationToken);
    }

    private Task PublishFailureEventAsync(AttachMediaToOwnerCommand request, string reason, string mediaFailureReason, CancellationToken cancellationToken)
    {

        var failedMedia = new Dictionary<Guid, string> { { request.MediaId, mediaFailureReason } };
        return publishEndpoint.Publish(
            new MediaAttachmentFailedIntegrationEvent(request.OwnerId, request.OwnerType, failedMedia, reason),
            cancellationToken);
    }
}