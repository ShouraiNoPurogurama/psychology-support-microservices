using Media.Application.Data;
using Media.Application.ServiceContracts;
using Media.Domain.Enums;
using Media.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Media.Application.EventHandlers
{
    public class MediaModerationHandler : INotificationHandler<MediaModerationRequestedEvent>
    {
        private readonly IMediaDbContext _dbContext;
        private readonly IStorageService _storageService;
        private readonly ISightengineService _sightengineService;

        public MediaModerationHandler(IMediaDbContext dbContext, IStorageService storageService, ISightengineService sightengineService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
            _sightengineService = sightengineService;
        }

        public async Task Handle(MediaModerationRequestedEvent notification, CancellationToken cancellationToken)
        {
            var mediaAsset = await _dbContext.MediaAssets
                .Include(m => m.MediaModerationAudits)
                .Include(m => m.MediaVariants)
                .FirstOrDefaultAsync(m => m.Id == notification.MediaId, cancellationToken);

            if (mediaAsset == null || !mediaAsset.MediaModerationAudits.Any(m => m.Status == MediaModerationStatus.Pending))
                return;

            var moderationAudit = mediaAsset.MediaModerationAudits.First(m => m.Status == MediaModerationStatus.Pending);
            var originalVariant = mediaAsset.MediaVariants.FirstOrDefault(v => v.VariantType == VariantType.Original);

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
                ContentType = mediaAsset.SourceMime
            };

            // Gửi file qua Sightengine
            var sightengineResult = await _sightengineService.CheckImageWithWorkflowAsync(file);

            moderationAudit.RawJson = sightengineResult.RawJson;
            moderationAudit.PolicyVersion = sightengineResult.WorkflowId;
            moderationAudit.Score = (decimal?)sightengineResult.Score;
            moderationAudit.CheckedAt = DateTime.UtcNow;

            if (sightengineResult.IsSafe)
            {
                moderationAudit.Status = MediaModerationStatus.Approved;
                mediaAsset.State = MediaState.Ready;
            }
            else
            {
                moderationAudit.Status = MediaModerationStatus.Rejected;
                mediaAsset.State = MediaState.Blocked;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
