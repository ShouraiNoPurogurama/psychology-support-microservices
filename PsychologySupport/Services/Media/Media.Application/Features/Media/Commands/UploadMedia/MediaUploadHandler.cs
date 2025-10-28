using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Media.Application.Data;
using Media.Application.Features.Media.Dtos;
using Media.Application.ServiceContracts;
using Media.Domain.Enums;
using Media.Domain.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace Media.Application.Features.Media.Commands.UploadMedia
{
    public record MediaUploadCommand(
        Guid IdempotencyKey,
        IFormFile File,
        MediaPurpose Purpose,
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
            ValidateFile(request.File);


            //Buffer file vào MemoryStream để có thể đọc nhiều lần
            await using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0; //reset vị trí stream về đầu

            var extractDimensionsTask = ExtractImageDimensionsAsync(request.File.ContentType, memoryStream);
            var calculateChecksumTask = CalculateChecksumAsync(memoryStream);

            var mediaId = Guid.NewGuid(); //Tạo ID trước để dùng trong blobKey
            var blobKey = GenerateBlobKey(request.MediaOwnerType, mediaId, request.File.FileName);
            var uploadTask = _storageService.UploadFileAsync(request.File, blobKey, request.File.ContentType, cancellationToken);

            await Task.WhenAll(extractDimensionsTask, calculateChecksumTask, uploadTask);

            var (width, height) = await extractDimensionsTask;
            var checksumSha256 = await calculateChecksumTask;
            var cdnUrl = await uploadTask;


            //dùng media id đã tạo ở trên
            var mediaAsset = MediaAsset.Create(
                mediaId,
                request.File.ContentType,
                request.File.Length,
                checksumSha256,
                request.Purpose,
                width,
                height);


            var format = DetermineMediaFormat(request.File.ContentType);
            mediaAsset.AddVariant(VariantType.Original, format, width ?? 0, height ?? 0, request.File.Length, blobKey, cdnUrl);

            //Add processing job
            if (mediaAsset.IsImage)
            {
                mediaAsset.AddProcessingJob(JobType.Thumbnail);
            }

            //Request moderation
            _dbContext.MediaAssets.Add(mediaAsset);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Giờ entity đã có trong DB
            mediaAsset.RequestModeration();
            await _dbContext.SaveChangesAsync(cancellationToken);


            //Build response
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

        private static async Task<(int? width, int? height)> ExtractImageDimensionsAsync(string contentType, MemoryStream stream)
        {
            if (!contentType.StartsWith("image/")) return (null, null);
            try
            {
                stream.Position = 0; //reset vị trí stream
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(stream);
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

        private static async Task<string> CalculateChecksumAsync(MemoryStream stream)
        {
            stream.Position = 0; //reset vị trí stream
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