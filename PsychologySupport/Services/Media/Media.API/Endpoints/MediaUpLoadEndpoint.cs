using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using Media.Application.Dtos;
using Media.Application.Media.Commands;
using Media.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Media.API.Endpoints
{
    public record MediaUploadRequest(
        IFormFile File,
        MediaOwnerType MediaOwnerType,
        Guid MediaOwnerId 
    );

    public record MediaUploadResponse(
        Guid MediaId,
        MediaState State,
        MediaVariantDto Original,
        MediaProcessingJobDto[] Jobs
    );

    public class MediaUploadEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/v1/media/uploads", async (
                [FromForm] MediaUploadRequest request,
                [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKeyHeader,
                ISender sender) =>
            {
                if (request.File == null || request.File.Length == 0)
                    throw new BadRequestException("File is required.");

                if (!Guid.TryParse(idempotencyKeyHeader, out var idempotencyKey))
                    throw new BadRequestException("Invalid X-Idempotency-Key header.");

                var command = new MediaUploadCommand(
                    idempotencyKey,
                    request.File,
                    request.MediaOwnerType,
                    request.MediaOwnerId
                );

                var result = await sender.Send(command);

                var response = result.Adapt<MediaUploadResponse>();

                return Results.Created($"/v1/media/{response.MediaId}", response);
            })
            .WithName("InitUpload")
            .WithTags("Media")
            .Produces<MediaUploadResponse>(201)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
            .ProducesProblem(StatusCodes.Status415UnsupportedMediaType)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
            .WithDescription("InitUpload")
            .WithSummary("InitUpload")
            .DisableAntiforgery();
        }
    }
}
