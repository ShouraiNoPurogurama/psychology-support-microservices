namespace Subscription.API.Services;

public interface IImageService
{
    Task<Guid> UploadImageAsync(IFormFile file, string ownerType, Guid ownerId);
    bool DeleteImageAsync(Guid imageId);
    string? GetImageUrlAsync(Guid imageId);
}