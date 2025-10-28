using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Media.Application.Data;
using Media.Application.Features.Media.Dtos;
using Media.Application.ServiceContracts;
using Media.Domain.Enums;
using Media.Domain.Models;
using System.Security.Cryptography;
using SixLabors.ImageSharp; // Cần thư viện này

namespace Media.Application.Features.Media.Commands.GenerateSticker;


public class GenerateStickerHandler : ICommandHandler<GenerateStickerCommand, GenerateStickerResult>
{
    private readonly IMediaDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly IStickerGenerationService _stickerService;

    public GenerateStickerHandler(
        IMediaDbContext dbContext,
        IStorageService storageService,
        IStickerGenerationService stickerService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _stickerService = stickerService;
    }

    public async Task<GenerateStickerResult> Handle(GenerateStickerCommand request, CancellationToken cancellationToken)
    {
        // === 1. GỌI GEMINI SERVICE ===
        await using var imageStream = await _stickerService.GenerateImageAsync(request.StickerGenerationPrompt, cancellationToken);
        
        // === LOGIC CỨNG (Hardcoded) ===
        // Handler này chỉ phục vụ 1 mục đích, nên các giá trị này là cố định
        const MediaOwnerType ownerType = MediaOwnerType.Reward;
        const MediaPurpose purpose = MediaPurpose.RewardSticker;
        const string mimeType = "image/webp";
        const MediaFormat format = MediaFormat.Webp;
        var mediaId = Guid.NewGuid(); // ID của asset mới

        // === 2. GỌI STORAGE SERVICE ===
        // Dùng RewardId (OwnerId) và MediaId mới để tạo blob key
        var blobKey = GenerateBlobKey(ownerType, request.RewardId, mediaId, "sticker.webp");
        var cdnUrl = await _storageService.UploadStreamAsync(imageStream, blobKey, mimeType, cancellationToken);

        // === 3. TÍNH TOÁN METADATA ===
        imageStream.Position = 0; 
        
        var extractDimensionsTask = ExtractImageDimensionsAsync(mimeType, imageStream);
        var calculateChecksumTask = CalculateChecksumAsync(imageStream);

        await Task.WhenAll(extractDimensionsTask, calculateChecksumTask);

        var (width, height) = await extractDimensionsTask;
        var checksumSha256 = await calculateChecksumTask;
        var fileSize = imageStream.Length; 

        // === 4. LƯU DB ===
        // Dùng hằng số 'purpose' đã định nghĩa
        var mediaAsset = MediaAsset.Create(
            mediaId,
            mimeType,
            fileSize,
            checksumSha256,
            purpose, // Hardcoded: MediaPurpose.RewardSticker
            width,
            height);
        
        // (Giả định: Sếp có thể muốn lưu OwnerId và OwnerType vào MediaAsset ở đây)
        // Ví dụ: mediaAsset.SetOwner(request.RewardId, ownerType);

        mediaAsset.AddVariant(VariantType.Original, format, width ?? 0, height ?? 0, fileSize, blobKey, cdnUrl);
        
        _dbContext.MediaAssets.Add(mediaAsset);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // === 5. TRẢ VỀ RESPONSE ===
        var originalVariant = mediaAsset.GetOriginalVariant()!;

        return new GenerateStickerResult(
            mediaAsset.Id,
            mediaAsset.State,
            new MediaVariantDto(
                originalVariant.Id,
                originalVariant.VariantType,
                originalVariant.Format,
                originalVariant.Width,
                originalVariant.Height,
                originalVariant.CdnUrl)
            );
    }
    
    // Blob key mới: {OwnerType}/{OwnerId}/{MediaId}/{fileName}
    // Ví dụ: Reward/guid-cua-reward-123/guid-cua-media-456/sticker.webp
    private static string GenerateBlobKey(MediaOwnerType ownerType, Guid ownerId, Guid mediaId, string fileName)
    {
        // Bỏ 'original' vì sticker chỉ có 1 phiên bản
        return $"{ownerType}/{ownerId}/{mediaId}/{fileName}"; 
    }
    
    private static async Task<(int? width, int? height)> ExtractImageDimensionsAsync(string contentType, Stream stream)
    {
        if (!contentType.StartsWith("image/")) return (null, null);
        try
        {
            stream.Position = 0; 
            var imageInfo = await SixLabors.ImageSharp.Image.IdentifyAsync(stream); 
            return (imageInfo.Width, imageInfo.Height);
        }
        catch (Exception ex)
        {
            throw new CustomValidationException(new Dictionary<string, string[]>
            {
                ["MEDIA_VALIDATION_FAILED"] = [ $"Invalid image stream from generation service. {ex.Message}" ]
            });
        }
    }

    private static async Task<string> CalculateChecksumAsync(Stream stream)
    {
        stream.Position = 0; 
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToBase64String(hash);
    }
}
