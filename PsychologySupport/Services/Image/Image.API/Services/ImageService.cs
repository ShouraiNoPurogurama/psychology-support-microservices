using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Image.API.Data;
using Image.API.Data.Common;
using Image.API.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace Image.API.Services
{
    public class ImageService : IImageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ImageDbContext _dbContext;
        private readonly string _containerName;
        private const long MaxFileSizeInBytes = 10 * 1024 * 1024; // 10 MB

        public ImageService(IConfiguration configuration, ImageDbContext dbContext)
        {
            _blobServiceClient = new BlobServiceClient(configuration["AzureBlobStorage:ConnectionString"]);
            _containerName = configuration["AzureBlobStorage:ContainerName"]!;
            _dbContext = dbContext;
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Vui lòng chọn ảnh để tải lên.");


            if (file.Length > MaxFileSizeInBytes)
                throw new ArgumentException($"Dung lượng ảnh không được vượt quá  {MaxFileSizeInBytes / (1024 * 1024)} MB.");

  
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!validExtensions.Contains(fileExtension))
                throw new ArgumentException("Chỉ chấp nhận các định dạng ảnh: JPG, JPEG, PNG, GIF, BMP.");

      
            try
            {
                using var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream());
                if (image == null)
                    throw new ArgumentException("Định dạng ảnh không hợp lệ. Vui lòng thử lại với ảnh khác.");
            }
            catch
            {
                throw new ArgumentException("Định dạng ảnh không hợp lệ. Vui lòng thử lại với ảnh khác.");
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, OwnerType ownerType, Guid ownerId)
        {
            ValidateFile(file); 

            string fileExtension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{fileExtension}";

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            string fileUrl = blobClient.Uri.ToString();

            var image = new Models.Image
            {
                Id = Guid.NewGuid(),
                Name = file.FileName,
                Url = fileUrl,
                OwnerType = ownerType,
                OwnerId = ownerId,
                Extension = fileExtension
            };

            _dbContext.Images.Add(image);
            await _dbContext.SaveChangesAsync();

            return fileUrl;
        }

        public async Task<string?> GetImageUrlAsync(OwnerType ownerType, Guid ownerId)
        {
            var image = await _dbContext.Images
                .Where(i => i.OwnerType == ownerType && i.OwnerId == ownerId)
                .FirstOrDefaultAsync();

            return image?.Url;
        }

        public async Task<string> UpdateImageAsync(IFormFile file, OwnerType ownerType, Guid ownerId)
        {
            ValidateFile(file); 

            var existingImage = await _dbContext.Images
                .Where(i => i.OwnerType == ownerType && i.OwnerId == ownerId)
                .FirstOrDefaultAsync();

            if (existingImage == null)
                throw new KeyNotFoundException($"Không tìm thấy ảnh cho đối tượng đã chọn.: {ownerType} với ID: {ownerId}.");

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = blobContainerClient.GetBlobClient(existingImage.Name);
            await blobClient.DeleteIfExistsAsync();

            string fileExtension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{fileExtension}";
            var newBlobClient = blobContainerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await newBlobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            string newFileUrl = newBlobClient.Uri.ToString();

            existingImage.Name = file.FileName;
            existingImage.Url = newFileUrl;
            existingImage.Extension = fileExtension;

            await _dbContext.SaveChangesAsync();

            return newFileUrl;
        }
    }
}
