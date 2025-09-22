using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Media.Application.Features.Media.Dtos;

namespace Media.Application.ServiceContracts
{
    public interface IStorageService
    {
        Task<PresignedUploadDto> GeneratePresignedUrlAsync(string blobName, string mimeType, TimeSpan expiry, CancellationToken cancellationToken);

        //Task<StorageObjectInfo> HeadObjectAsync(string blobName, CancellationToken cancellationToken);
        Task<Stream> GetObjectAsync(string blobName, CancellationToken cancellationToken);
        Task<string> GetCdnUrlAsync(string blobName, CancellationToken cancellationToken);

        Task<string> UploadFileAsync(IFormFile file, string blobName, string mimeType, CancellationToken cancellationToken);

        Task<Stream> DownloadFileAsync(string blobKey);
    }

}
