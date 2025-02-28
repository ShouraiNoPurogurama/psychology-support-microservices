namespace Subscription.API.Services;

public interface IImageService
{
    Task<Guid> UploadImageAsync(IFormFile file, string ownerType, Guid ownerId);
    Task<bool> DeleteImageAsync(Guid imageId);
    Task<string?> GetImageUrlAsync(Guid imageId);
}