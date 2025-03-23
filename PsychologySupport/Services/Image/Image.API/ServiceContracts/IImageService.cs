using Image.API.Data.Common;

namespace Image.API.ServiceContracts
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file, OwnerType ownerType, Guid ownerId);
        Task<string> UpdateImageAsync(IFormFile file, OwnerType ownerType, Guid ownerId);
        Task<string> GetImageUrlAsync(OwnerType ownerType, Guid ownerId);
    }
}
