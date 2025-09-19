using BuildingBlocks.CQRS;
using BuildingBlocks.DDD;
using BuildingBlocks.Exceptions;
using Media.Application.Data;
using Media.Application.Dtos;
using Media.Application.ServiceContracts;
using Media.Domain.Enums;
using Media.Domain.Models;
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

        public MediaUploadHandler(IMediaDbContext dbContext, IStorageService storageService)
        {
            _dbContext = dbContext;
            _storageService = storageService;
        }

        public async Task<MediaUploadResult> Handle(MediaUploadCommand request, CancellationToken cancellationToken)
        {
            // Validation
            ValidateFile(request.File);
            
            // Extract image dimensions if it's an image
            var (width, height) = await ExtractImageDimensionsAsync(request.File);
            
            // Calculate checksum
            var checksumSha256 = await CalculateChecksumAsync(request.File);

            // Create domain aggregate using factory method
            var mediaAsset = MediaAsset.Create(
                request.File.ContentType,
                request.File.Length,
                checksumSha256,
                width,
                height);

            // Assign ownership
            mediaAsset.AssignOwnership(request.MediaOwnerType, request.MediaOwnerId);

            // Upload file to storage
            var blobKey = GenerateBlobKey(request.MediaOwnerType, mediaAsset.Id, request.File.FileName);
            var cdnUrl = await _storageService.UploadFileAsync(request.File, blobKey, request.File.ContentType, cancellationToken);

            // Create original variant
            var format = DetermineMediaFormat(request.File.ContentType);
            mediaAsset.AddVariant(VariantType.Original, format, width ?? 0, height ?? 0, request.File.Length, blobKey, cdnUrl);

            // Add processing job
            if (mediaAsset.IsImage)
            {
                mediaAsset.AddProcessingJob(JobType.Thumbnail);
            }

            // Request moderation
            mediaAsset.RequestModeration();

            // Persist to database
            _dbContext.MediaAssets.Add(mediaAsset);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Build response
            var originalVariant = mediaAsset.GetOriginalVariant()!;
            var processingJobs = mediaAsset.ProcessingJobs
                .Where(j => !j.IsFinished)
                .Select(j => new MediaProcessingJobDto(j.Id, j.JobType, j.Status))
                .ToArray();

            return new MediaUploadResult(
                mediaAsset.Id,
                mediaAsset.State,
                new MediaVariantDto(
                    originalVariant.Id,
                    originalVariant.VariantType,
                    originalVariant.Format,
                    originalVariant.Width,
                    originalVariant.Height,
                    originalVariant.CdnUrl),
                processingJobs);
        }

        private static void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new CustomValidationException(new Dictionary<string, string[]>
                {
                    ["MEDIA_FILE_REQUIRED"] = new[] { "File is required." }
                });

            if (file.Length > MaxFileSizeInBytes)
                throw new CustomValidationException(new Dictionary<string, string[]>
                {
                    ["MEDIA_TOO_LARGE"] = new[] { "File exceeds maximum size limit." }
                });
        }

        private static async Task<(int? width, int? height)> ExtractImageDimensionsAsync(IFormFile file)
        {
            if (!file.ContentType.StartsWith("image/"))
                return (null, null);

            try
            {
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream());
                return (image.Width, image.Height);
            }
            catch
            {
                throw new CustomValidationException(new Dictionary<string, string[]>
                {
                    ["MEDIA_VALIDATION_FAILED"] = new[] { "Invalid image file." }
                });
            }
        }

        private static async Task<string> CalculateChecksumAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var sha256 = SHA256.Create();
            var hash = await sha256.ComputeHashAsync(stream);
            return Convert.ToBase64String(hash);
        }

        private static string GenerateBlobKey(MediaOwnerType ownerType, Guid mediaId, string fileName)
        {
            return $"{ownerType}/{mediaId}/original/{fileName}";
        }

        private static MediaFormat DetermineMediaFormat(string contentType)
        {
            return contentType.Split('/')[1].ToLowerInvariant() switch
            {
                "jpeg" or "jpg" => MediaFormat.Jpeg,
                "png" => MediaFormat.Png,
                "webp" => MediaFormat.Webp,
                "avif" => MediaFormat.Avif,
                _ => throw new CustomValidationException(new Dictionary<string, string[]>
                {
                    ["MEDIA_UNSUPPORTED_FORMAT"] = new[] { "Unsupported media format." }
                })
            };
        }
    }
}