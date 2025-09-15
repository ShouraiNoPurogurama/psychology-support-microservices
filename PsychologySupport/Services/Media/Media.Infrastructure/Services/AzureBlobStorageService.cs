using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using BuildingBlocks.Exceptions;
using Media.Application.Dtos;
using Media.Application.ServiceContracts;
using Media.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Media.Infrastructure.Services;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly string _cdnBaseUrl;

    public AzureBlobStorageService(
        BlobServiceClient blobServiceClient,
        IOptions<AzureStorageOptions> options)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = options.Value.ContainerName;
        _cdnBaseUrl = options.Value.CdnBaseUrl;
    }

    public async Task<PresignedUploadDto> GeneratePresignedUrlAsync(
        string blobName,
        string mimeType,
        TimeSpan expiry,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new BadRequestException("Tên tệp (blobName) không được để trống.");

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiry),
                ContentType = mimeType
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Write);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            return new PresignedUploadDto
            {
                UploadUrl = sasUri.ToString(),
                ExpiresAt = sasBuilder.ExpiresOn.UtcDateTime
            };
        }
        catch (Azure.RequestFailedException ex)
        {
            throw new InternalServerException(
                "Không thể kết nối tới Azure Storage.",
                ex.Message);
        }
        catch (Exception ex)
        {
            throw new InternalServerException(
                "Lỗi hệ thống khi tạo URL tải lên.",
                ex.Message);
        }
    }

    public async Task<Stream> GetObjectAsync(string blobName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new BadRequestException("Tên tệp (blobName) không được để trống.");

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            return response.Value.Content;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            throw new NotFoundException("Không tìm thấy tệp trong kho lưu trữ.");
        }
        catch (Azure.RequestFailedException ex)
        {
            throw new InternalServerException(
                "Không thể truy cập vào Azure Blob.",
                ex.Message);
        }
    }

    public Task<string> GetCdnUrlAsync(string blobName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new BadRequestException("Tên tệp (blobName) không được để trống.");

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var url = string.IsNullOrEmpty(_cdnBaseUrl)
            ? blobClient.Uri.ToString()
            : $"{_cdnBaseUrl}/{blobName}";

        return Task.FromResult(url);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string blobName, string mimeType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new BadRequestException("Tên tệp (blobName) không được để trống.");

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = mimeType }, cancellationToken: cancellationToken);
            }

            return await GetCdnUrlAsync(blobName, cancellationToken);
        }
        catch (Azure.RequestFailedException ex)
        {
            throw new InternalServerException(
                "Không thể tải file lên Azure Blob Storage.",
                ex.Message);
        }
    }

    public async Task<Stream> DownloadFileAsync(string blobKey)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobKey);
        return await blobClient.OpenReadAsync();
    }
}