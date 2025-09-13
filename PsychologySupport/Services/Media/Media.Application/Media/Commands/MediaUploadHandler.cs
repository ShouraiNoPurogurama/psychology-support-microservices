using BuildingBlocks.CQRS;
using BuildingBlocks.DDD;
using BuildingBlocks.Exceptions;
using Media.API.Media.Models;
using Media.Application.Data;
using Media.Application.Dtos;
using Media.Application.ServiceContracts;
using Media.Domain.Enums;
using Media.Domain.Events;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace Media.Application.Media.Commands
{
    public record MediaUploadCommand(
        Guid IdempotencyKey,
        IFormFile File,
        MediaOwnerType MediaOwnerType,
        Guid MediaOwnerId
    ) : IdempotentCommand<MediaUploadResult>(IdempotencyKey);

    public record MediaUploadResult(
        Guid MediaId,
        MediaState State,
        MediaVariantDto Original,
        MediaProcessingJobDto[] Jobs
    );
    public class MediaUploadHandler : ICommandHandler<MediaUploadCommand, MediaUploadResult>
    {
        private readonly IMediaDbContext _dbContext;
        private readonly IStorageService _storageService;

        private const long MaxFileSizeInBytes = 100 * 1024 * 1024;

        private int width = 0;
        private int height = 0;

        public MediaUploadHandler(IMediaDbContext dbContext, IStorageService storageService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
        }

        public async Task<MediaUploadResult> Handle(MediaUploadCommand request, CancellationToken cancellationToken)
        {

            // Idempotency check

            // Validation
            long sourceBytes = request.File.Length;

            if (request.File == null || request.File.Length == 0)
                throw new CustomValidationException(new Dictionary<string, string[]>
                {
                    ["MEDIA_FILE_REQUIRED"] = new[] { "File is required." }
                });

            if (request.File.Length > MaxFileSizeInBytes)
                throw new CustomValidationException(new Dictionary<string, string[]>
                {
                    ["MEDIA_TOO_LARGE"] = new[] { "Vượt kích thước tối đa." }
                });


            // Validate image format 
            string mimeType = request.File.ContentType;

            if (mimeType.StartsWith("image/"))
            {
                try
                {
                    using var image = SixLabors.ImageSharp.Image.Load(request.File.OpenReadStream());
                    if (image == null)
                        throw new CustomValidationException(new Dictionary<string, string[]>
                        {
                            ["MEDIA_VALIDATION_FAILED"] = new[] { "File ảnh không hợp lệ." }
                        });

                        width = image.Width;
                        height = image.Height;
                }
                catch
                {
                    throw new CustomValidationException(new Dictionary<string, string[]>
                    {
                        ["MEDIA_VALIDATION_FAILED"] = new[] { "File ảnh không hợp lệ." }
                    });
                }

            
            }

            
            // Tính checksum_sha256

            string checksumSha256;
            using (var stream = request.File.OpenReadStream())
            {
                using var sha256 = SHA256.Create();
                var hash = await sha256.ComputeHashAsync(stream);
                checksumSha256 = Convert.ToBase64String(hash);
                stream.Position = 0;
            }

            #region Duplicate Check
            // Kiểm tra trùng lặp
            //var existingMedia = await _dbContext.MediaAssets
            //    .FirstOrDefaultAsync(m => m.ChecksumSha256 == checksumSha256 && m.State == MediaState.Ready, cancellationToken);
            //if (existingMedia != null)
            //{
            //    var existingVariant = await _dbContext.MediaVariants
            //        .FirstOrDefaultAsync(v => v.MediaId == existingMedia.Id && v.VariantType == VariantType.Original, cancellationToken);
            //    return new MediaUploadResult(
            //        existingMedia.Id,
            //        existingMedia.State,
            //        new MediaVariantDto(
            //            existingVariant.Id,
            //            existingVariant.VariantType,
            //            existingVariant.Format,
            //            existingVariant.Width,
            //            existingVariant.Height,
            //            existingVariant.CdnUrl
            //        ),
            //        Array.Empty<MediaProcessingJobDto>()
            //    );
            //}
            #endregion

            // Tạo media asset
            var mediaId = Guid.NewGuid();
            var blobKey = $"{request.MediaOwnerType.ToString()}/{mediaId}/original/{request.File.FileName}";
            var cdnUrl = await _storageService.UploadFileAsync(request.File, blobKey, mimeType, cancellationToken);

            var mediaAsset = new MediaAsset
            {
                Id = mediaId,
                State = MediaState.Processing,
                SourceMime = mimeType,
                SourceBytes = request.File.Length,
                ChecksumSha256 = checksumSha256,
                Width = width,
                Height = height,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.MediaAssets.Add(mediaAsset);

            var moderationAudit = new MediaModerationAudit
            {
                Id = Guid.NewGuid(),
                MediaId = mediaId,
                Status = MediaModerationStatus.Pending
            };
            _dbContext.MediaModerationAudits.Add(moderationAudit);

            // Tạo original variant
            var variantId = Guid.NewGuid();
            var variant = new MediaVariant
            {
                Id = variantId,
                MediaId = mediaId,
                VariantType = VariantType.Original,
                Format = Enum.TryParse<MediaFormat>(mimeType.Split('/')[1], true, out var format)
                    ? format
                    : throw new CustomValidationException(new Dictionary<string, string[]>
                    {
                        ["MEDIA_UNSUPPORTED_FORMAT"] = new[] { "Unsupported media format." }
                    }),
                BucketKey = blobKey,
                CdnUrl = cdnUrl,
                Width = width,
                Height = height
            };
            _dbContext.MediaVariants.Add(variant);


            if (request.MediaOwnerType != null && request.MediaOwnerId !=null)
            {
                _dbContext.MediaOwners.Add(new MediaOwner
                {
                    Id = Guid.NewGuid(),
                    MediaId = mediaId,
                    MediaOwnerType = request.MediaOwnerType,
                    MediaOwnerId = request.MediaOwnerId
                });
            }

           // Thêm job xử lý nền
           var jobId = Guid.NewGuid();
            var processingJob = new MediaProcessingJob
            {
                Id = jobId,
                MediaId = mediaId,
                JobType = JobType.Thumbnail,
                Status = ProcessStatus.Queued
            };
            _dbContext.MediaProcessingJobs.Add(processingJob);

            var responseBody = new MediaUploadResult(
                mediaId,
                MediaState.Processing,
                new MediaVariantDto(
                    variantId,
                    variant.VariantType,
                    variant.Format,
                    variant.Width,
                    variant.Height,
                    variant.CdnUrl
                ),
                new[] { new MediaProcessingJobDto(jobId, JobType.Thumbnail, ProcessStatus.Queued)}
            );


            await _dbContext.SaveChangesAsync(cancellationToken);

            // Domain Event 
            mediaAsset.AddDomainEvent(new MediaModerationRequestedEvent(mediaId));

            await _dbContext.SaveChangesAsync(cancellationToken);

            return responseBody;
        }

    }
}