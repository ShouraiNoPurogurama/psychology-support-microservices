using Carter;
using Mapster;
using Media.API.Media.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Media.API.Media.Features.MediaUpload
{
    public record MediaUploadInitApiRequest(MediaUploadInitRequestDto Request);
    public record MediaUploadInitApiResponse(string UploadId, string MediaTempId, PresignedDto Presigned);

    public class MediaUpLoadEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("v1/media/uploads:init", async ([FromBody] MediaUploadInitApiRequest request, ISender sender, HttpContext httpContext) =>
                {
                    // Authorization check (implement as needed)
                    // if (!AuthorizationHelpers.CanUploadMedia(request.Request.Owner.MediaOwnerId, httpContext.User))
                    //     throw new ForbiddenException();

                    var command = request.Adapt<MediaUploadInitCommand>();
                    var result = await sender.Send(command);
                    var response = result.Adapt<MediaUploadInitApiResponse>();

                    return Results.Ok(response);
                })
                .WithName("InitMediaUpload")
                .WithTags("MediaUpload")
                .Produces<MediaUploadInitApiResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Init Media Upload")
                .WithSummary("Init Media Upload");
        }
    }
}
