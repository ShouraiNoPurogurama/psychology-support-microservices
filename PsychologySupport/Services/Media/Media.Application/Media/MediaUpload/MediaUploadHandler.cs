//using Azure.Storage.Blobs;
//using Azure.Storage.Sas;
//using BuildingBlocks.CQRS;
//using Media.API.Media.Dtos;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Media.API.Media.Features.MediaUpload
//{
//    public record MediaUploadInitCommand(MediaUploadInitRequestDto Request) : ICommand<MediaUploadInitResult>;

//    public record MediaUploadInitResult(
//        string UploadId,
//        string MediaTempId,
//        PresignedDto Presigned
//    );
//    public class MediaUploadHandler : ICommandHandler<MediaUploadInitCommand, MediaUploadInitResult>
//    {
//        private readonly BlobServiceClient _blobServiceClient;
//        private readonly string _containerName;

//        public MediaUploadHandler(BlobServiceClient blobServiceClient, IConfiguration configuration)
//        {
//            _blobServiceClient = blobServiceClient;
//            _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "media-uploads";
//        }

//        public async Task<MediaUploadInitResult> Handle(MediaUploadInitCommand command, CancellationToken cancellationToken)
//        {
//            var request = command.Request;

//            var uploadId = $"ul_{Guid.NewGuid()}";
//            var mediaTempId = $"mt_{Guid.NewGuid()}";
//            var blobName = $"{uploadId}/{request.FileName}";

//            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
//            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

//            var blobClient = containerClient.GetBlobClient(blobName);

//            var sasBuilder = new BlobSasBuilder
//            {
//                BlobContainerName = _containerName,
//                BlobName = blobName,
//                Resource = "b",
//                ExpiresOn = DateTimeOffset.UtcNow.AddSeconds(900)
//            };
//            sasBuilder.SetPermissions(BlobSasPermissions.Write);

//            var sasUri = blobClient.GenerateSasUri(sasBuilder);

//            var presigned = new PresignedDto
//            {
//                Url = sasUri.ToString(),
//                Headers = new Dictionary<string, string> { { "Content-Type", request.MimeType } },
//                ExpiresIn = 900
//            };

//            return new MediaUploadInitResult(uploadId, mediaTempId, presigned);
//        }
//    }
//}
