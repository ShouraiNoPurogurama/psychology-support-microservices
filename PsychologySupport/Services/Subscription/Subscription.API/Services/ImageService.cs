using BuildingBlocks.Messaging.Events.IntegrationEvents.ImageStorage;
using MassTransit;

namespace Subscription.API.Services;

public class ImageService : IImageService
{
    private readonly string _imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    private readonly ILogger<ImageService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public ImageService(ILogger<ImageService> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;

        // Ensure directory exists
        if (!Directory.Exists(_imageDirectory)) Directory.CreateDirectory(_imageDirectory);
    }

    public async Task<Guid> UploadImageAsync(IFormFile file, string ownerType, Guid ownerId)
    {
        var imageId = Guid.NewGuid();
        var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
        var fileName = $"{imageId}.{fileExtension}";

        byte[] imageBytes;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            imageBytes = memoryStream.ToArray();
        }

        // Gửi sự kiện lên Message Bus để ImageStorage service xử lý
        var imageEvent = new ImageUploadedIntegrationEvent(
            imageId,
            fileName,
            imageBytes,
            fileExtension,
            ownerType,
            ownerId
        );

        await _publishEndpoint.Publish(imageEvent);

        return imageId;
    }

    public bool DeleteImageAsync(Guid imageId)
    {
        var imagePath = Directory.GetFiles(_imageDirectory, $"{imageId}.*").FirstOrDefault();
        if (imagePath is null) return false;

        try
        {
            File.Delete(imagePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xảy ra lỗi khi xóa ảnh có ID: {ImageId}", imageId);
            return false;
        }
    }

    public string? GetImageUrlAsync(Guid imageId)
    {
        var imagePath = Directory.GetFiles(_imageDirectory, $"{imageId}.*").FirstOrDefault();
        if (imagePath is null) return null;

        return $"/uploads/{Path.GetFileName(imagePath)}";
    }
}