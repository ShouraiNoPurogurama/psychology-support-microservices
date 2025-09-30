using Media.Application.Data;
using Media.Application.ServiceContracts;
using Media.Domain.Enums;
using Media.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Media.Application.Features.Media.EventHandlers
{
    public class MediaModerationHandler : INotificationHandler<MediaModerationRequestedEvent>
    {
        private readonly IMediaDbContext _dbContext;
        private readonly IStorageService _storageService;
        private readonly ISightengineService _sightengineService;

        public MediaModerationHandler(IMediaDbContext dbContext, IStorageService storageService,
            ISightengineService sightengineService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
            _sightengineService = sightengineService;
        }

        public async Task Handle(MediaModerationRequestedEvent notification, CancellationToken cancellationToken)
        {
            var mediaAsset = await _dbContext.MediaAssets
                .Include(m => m.ModerationAudits)
                .Include(m => m.Variants)
                .FirstOrDefaultAsync(m => m.Id == notification.MediaId, cancellationToken);

            if (mediaAsset == null || !mediaAsset.Moderation.IsPending)
                return;

            //var moderationAudit = mediaAsset.ModerationAudits.First(m => m.Status == MediaModerationStatus.Pending);
            var originalVariant = mediaAsset.Variants.FirstOrDefault(v => v.VariantType == VariantType.Original);

            if (originalVariant == null)
                throw new Exception("Không tìm thấy variant gốc để kiểm tra moderation.");

            // Tải file từ Azure Blob Storage 
            using var fileStream = await _storageService.DownloadFileAsync(originalVariant.BucketKey);
            if (fileStream == null)
                throw new Exception("Không thể tải file từ storage.");

            // Tạo IFormFile từ stream
            var file = new FormFile(fileStream, 0, fileStream.Length, "media", originalVariant.BucketKey.Split('/').Last())
            {
                Headers = new HeaderDictionary(),
                ContentType = mediaAsset.Content.MimeType
            };

            // Gửi file qua Sightengine
            var sightengineResult = await _sightengineService.CheckImageWithWorkflowAsync(file);

            var auditStatus = sightengineResult.IsSafe ? MediaModerationStatus.Approved : MediaModerationStatus.Rejected;
            
            //moderationAudit.UpdateStatus(auditStatus, (decimal?)sightengineResult.Score, sightengineResult.WorkflowId,
            //    sightengineResult.RawJson);

            if (sightengineResult.IsSafe)
            {
                // Update moderation và auto mark as ready
                mediaAsset.ApproveModeration(
                    policyVersion: sightengineResult.WorkflowId,
                    score: (decimal?)sightengineResult.Score,
                    rawJson: sightengineResult.RawJson
                );
            }
            else
            {
                mediaAsset.RejectModeration(
                    policyVersion: sightengineResult.WorkflowId,
                    score: (decimal?)sightengineResult.Score,
                    rawJson: sightengineResult.RawJson
                );
            }


            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}